using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Core.Cards;
using Core.Cards.Effects;
using Core.Contracts;
using Core.Map.Server;
using Core.Server;
using Core.Utils;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Match.Server
{
    public partial class MatchServer
    {
        private bool isPve;

        public bool SaveNextTurn { get; private set; }

        public bool DiscardNextTurn { get; private set; }

        private readonly TurnTimer turnTimer;

        private MatchState matchState = MatchState.Created;

        public event EventHandler OnTurnPassed;

        public event EventHandler OnMatchStarted;

        public event EventHandler OnFatigueDamaged;

        public event EventHandler BeforeFatigueDamaged;

        public Predicate<MatchPlayer> FatigueFilter;

        public Guid MatchId { get; private set; }

        public MatchDetails MatchDetails { get; private set; }

        private MatchServer(bool isPve)
        {
            Debug.Log("Match creating");
            MatchId = Guid.NewGuid();
            MatchDetails = new MatchDetails();
            this.isPve = isPve;
            MatchDetails.TurnTime = int.Parse(Configurator.data["BattleConfiguration"]["turnTime"]);
            MatchDetails.FatigueTurn = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]);
            MatchDetails.PrepareTime = 5;
            turnTimer = new TurnTimer(this);
        }

        public void AddPlayer(string playFabId, string name, PlayerCards cards, int division,
            NetworkConnectionToClient connection)
        {
            MatchPlayer player = new MatchPlayer()
            {
                PlayFabId = playFabId, PlayerCards = new PlayerCards(cards.CardsIdDeck), Name = name,
                Division = division, Connection = connection
            };
            MatchDetails.Players.Add(player);
        }

        public void AddBot(PlayerCards cards, string name)
        {
            MatchBot bot = new MatchBot(this) {PlayerCards = new PlayerCards(cards.CardsIdDeck), Name = name};
            bot.Division = MatchDetails.Players.FirstOrDefault(p => p is not IMatchBot)?.Division ?? 10;
            MatchDetails.Players.Add(bot);
        }

        public void Start()
        {
            Debug.Log("Match starting");
            matchState = MatchState.Game;
            MatchDetails.Division = MatchDetails.Players.Where(p => p is not IMatchBot).Select(p => p.Division).Min();
            foreach (var player in MatchDetails.Players)
            {
                player.Castle = new DivisionCastleCreator(MatchDetails.Division).CreateCastle();
                try
                {
                    player.PlayerCards!.FillHand();
                }
                catch (NullReferenceException e)
                {
                    Debug.LogError("PlayerCards on match start is null!");
                    throw;
                }
            }

            MatchDetails.Fatigue = new Fatigue(MatchDetails.Division);
            MatchDetails.Turn = 0;
            if (MatchDetails.LevelInfo == null)
                MatchDetails.LevelInfo = new();

            turnTimer.Start();
            OnMatchStarted?.Invoke(this, EventArgs.Empty);
            SendOutMatchDetails();
            OnTurnPassed?.Invoke(this, EventArgs.Empty);
        }

        public void HandlePlayCardRequest(PlayerData playerData, CardData cardData)
        {
            if (matchState != MatchState.Game || MatchDetails.Players.Count <= 1)
                return;

            Debug.Log("Playing card");
            if ((DiscardNextTurn && !SaveNextTurn) || matchState != MatchState.Game)
                return;

            MatchPlayer matchPlayer = MatchDetails.CurrentPlayer;
            if (matchPlayer == null || matchPlayer.PlayFabId != playerData.PlayFabId)
                return;

            if (!TurnValidator.ValidateCardTurn(matchPlayer, Guid.Parse(cardData.Id), cardData))
            {
                Debug.Log("This card move is illegal");
                HandleImpossibleMove();
                return;
            }

            PlayCard(cardData);
            NotifyClientsAboutPlayedCard(cardData, playerData.PlayFabId);
            SendOutMatchDetails();
        }

        private void PlayCard(CardData cardData)
        {
            if (matchState != MatchState.Game || MatchDetails.Players.Count <= 1)
            {
                Debug.Log("Match state is not eligible to play a card");
                return;
            }

            foreach (var res in cardData.Cost)
            {
                MatchDetails.CurrentPlayer?.Castle.GetResource(res.Name).RemoveResource(res.Value);
            }

            SaveNextTurn = cardData.SaveTurn;
            foreach (var effect in cardData.Effects)
            {
                if (effect is DiscardEffect)
                {
                    DiscardNextTurn = true;
                }

                effect.Execute(MatchDetails.CurrentPlayer, MatchDetails.NextPlayer);
            }

            MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();

            PassTheMove();
        }

        public void HandleDiscardCardRequest(PlayerData playerData, CardData cardData)
        {
            if (matchState != MatchState.Game || MatchDetails.Players.Count <= 1)
            {
                Debug.Log("Match state is not eligible to discard a card");
                return;
            }

            MatchPlayer matchPlayer = MatchDetails.CurrentPlayer;
            if (matchPlayer == null || matchPlayer.PlayFabId != playerData.PlayFabId)
            {
                Debug.Log("Card discard error: player is NULL");
                return;
            }

            if (!TurnValidator.ValidateCardInHand(matchPlayer, Guid.Parse(cardData.Id)) || cardData.NonDiscard)
            {
                Debug.Log("Card you want to discard is not in hand!");
                HandleImpossibleMove();
                return;
            }

            if (DiscardNextTurn)
            {
                Debug.Log("Free discard!");
                DiscardNextTurn = false;
                MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
                MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.RequestDiscard);
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.Draft);
                SendOutMatchDetails();
                return;
            }

            if (SaveNextTurn)
            {
                Debug.Log("Free turn discard!");
                SaveNextTurn = false;
                MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
                MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.RequestDiscard);
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.Draft);
                SendOutMatchDetails();
                return;
            }

            Debug.Log("Turn discard!");
            MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();

            PassTheMove();

            //NotifyClientsAboutCardDiscard(cardData, pla);
            SendOutMatchDetails();
        }

        public void PassTheMove(bool force = false)
        {
            if (SaveNextTurn && !force)
            {
                SaveNextTurn = false;
                OnTurnPassed?.Invoke(this, EventArgs.Empty);
                Debug.Log("Free move!");
                return;
            }

            SaveNextTurn = false;

            if (force)
            {
                Debug.Log("Turn passing is forced!");
                DiscardNextTurn = false;
            }

            turnTimer.Start();

            MatchDetails.Turn++;

            if (MatchDetails.Turn != 0)
            {
                if (MatchDetails.Turn % 2 == 0)
                {
                    if (MatchDetails.Turn >= MatchDetails.FatigueTurn)
                    {
                        DamageFatigue();
                        MatchDetails.Fatigue++;
                    }

                    foreach (var player in MatchDetails.Players)
                    {
                        player.Castle.Resources.ForEach(r => r.AddResource(r.Income));
                    }
                }
            }

            OnTurnPassed?.Invoke(this, EventArgs.Empty);
        }

        private void HandleImpossibleMove()
        {
            if (matchState != MatchState.Game)
                return;

            Debug.Log("Impossible move catched!");
            SendOutMatchDetails();
        }

        public void SendOutMatchDetails()
        {
            if (matchState != MatchState.Game)
                return;

            foreach (var player in MatchDetails.Players)
            {
                var matchDetailsDto = CreateMatchDetailsDto(player);
                Debug.Log("Match details to send:\n" + JsonConvert.SerializeObject(matchDetailsDto));
                player.Connection?.Send(matchDetailsDto);
            }

            ValidateGameState();
        }

        public void NotifyClientsAboutPlayedCard(CardData cardData, string playerId)
        {
            if (matchState != MatchState.Game)
                return;

            foreach (var player in MatchDetails.Players)
            {
                RequestCardDto dto = new()
                {
                    AccountId = Guid.Empty,
                    CardId = Guid.Parse(cardData.Id),
                    ActionType = player.PlayFabId == playerId ? CardActionType.YouPlayed : CardActionType.EnemyPlayed
                };
                Debug.Log("Played card info to send\n" + JsonConvert.SerializeObject(dto));
                player.Connection?.Send(dto);
            }
        }

        private MatchDetailsDto CreateMatchDetailsDto(MatchPlayer player)
        {
            return new()
            {
                PlayerId = player.PlayFabId,
                Players = MatchDetails.Players.Select(p => new MatchPlayerDto()
                {
                    Castle = p.Castle, Name = p.Name, PlayerId = p.PlayFabId == player.PlayFabId ? p.PlayFabId : ""
                }).ToArray(),
                Fatigue = MatchDetails.Fatigue,
                IsYourTurn = MatchDetails.CurrentPlayer?.PlayFabId == player.PlayFabId,
                CardsInHandIds = player?.PlayerCards?.CardsIdHand?.Select(c => c.ToString())?.ToArray(),
                LevelInfo = MatchDetails.LevelInfo
            };
        }

        private void DamageFatigue()
        {
            if (matchState != MatchState.Game)
                return;

            Debug.Log($"Fatigue is damaging for {MatchDetails.Fatigue.Damage}");
            foreach (var player in MatchDetails.Players)
            {
                bool notDamagePlayer = FatigueFilter?.GetInvocationList().All(predicate => !((Predicate<MatchPlayer>) predicate)(player)) ?? false;

                if (!notDamagePlayer)
                {
                    BeforeFatigueDamaged?.Invoke(this, EventArgs.Empty);
                    DamagePlayer(player, MatchDetails.Fatigue.Damage, false);
                    OnFatigueDamaged?.Invoke(this, EventArgs.Empty);
                }

                player.Connection?.Send(new FatigueDto()
                {
                    PlayerId = Guid.Empty,
                    Damage = Math.Min(MatchDetails.Fatigue.Damage + MatchDetails.Fatigue.Income,
                        MatchDetails.Fatigue.MaxDamage)
                });
            }
        }

        public void DamagePlayer(MatchPlayer player, int damage, bool spread = true)
        {
            if (matchState != MatchState.Game)
                return;

            if (!spread)
            {
                if (player.Castle.Wall.Health > 0)
                {
                    player.Castle.Wall.Damage(damage);
                }
                else
                {
                    player.Castle.Tower.Damage(damage);
                }

                return;
            }

            int towerDamage = Math.Max(0, damage - player.Castle.Wall.Health);
            player.Castle.Tower.Damage(towerDamage);
            player.Castle.Wall.Damage(damage - towerDamage);
        }

        public void Stop()
        {
            matchState = MatchState.Ended;
            MatchServerController.RemoveMatch(MatchId);
            turnTimer.Close();
        }

        private void ValidateGameState()
        {
            List<MatchPlayer> winPlayers = new();
            List<MatchPlayer> losePlayers = new();
            foreach (var player in MatchDetails.Players)
            {
                Debug.Log(
                    $"Validation player info:\nMaxHp: {player.Castle.Tower.MaxHealth} Hp: {player.Castle.Tower.Health}");
                if (player.Castle.Tower.Health >= player.Castle.Tower.MaxHealth)
                {
                    Debug.Log("Player wins by castle!");
                    winPlayers.Add(player);
                }

                if (player.Castle.Tower.Health <= 0)
                {
                    Debug.Log("Player lose by castle!");
                    losePlayers.Add(player);
                }
            }

            winPlayers.ForEach(p =>
            {
                ApplyAfterGameStatistics(p, true);
                SendGameResult(p, true);
                MatchDetails.Players.Remove(p);
            });
            losePlayers.ForEach(p =>
            {
                ApplyAfterGameStatistics(p, false);
                SendGameResult(p, false);
                MatchDetails.Players.Remove(p);
            });

            if (MatchDetails.Players.Count < 2)
            {
                if (MatchDetails.Players.Count == 1)
                {
                    var p = MatchDetails.Players[0];
                    ApplyAfterGameStatistics(p, winPlayers.Count == 0);
                    SendGameResult(p, winPlayers.Count == 0);
                    MatchDetails.Players.Remove(p);
                }

                Stop();
            }
        }

        private void SendGameResult(MatchPlayer player, bool isWin)
        {
            RequestMatchDto dto = new()
            {
                AccountId = Guid.Empty,
                // ReSharper disable once Unity.NoNullPropagation
                LevelId = MatchDetails.LevelInfo?.LevelId ?? -1,
                RequestType = isWin ? MatchRequestType.WinMatch : MatchRequestType.LoseMatch
            };
            player.Connection?.Send(dto);
        }

        private void ApplyAfterGameStatistics(MatchPlayer player, bool isWin)
        {
            if (isWin)
            {
                if (MatchDetails.LevelInfo != null && MatchDetails.MapProgress != null &&
                    !string.IsNullOrEmpty(player.PlayFabId))
                {
                    ServerMapController.Instance.UpdateMapProgress(MatchDetails.MapProgress,
                        MatchDetails.LevelInfo, player.PlayFabId);
                }
                //Update play fab wins count statistic here
            }
        }
    }

    public enum MatchState
    {
        Created,
        Game,
        Ended,
    }
}