using System;
using System.Collections.Generic;
using System.Linq;
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

        private bool saveNextTurn;

        private bool discardNextTurn;

        private MatchState matchState = MatchState.Created;

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
        }

        public void AddPlayer(string playFabId, string name, PlayerCards cards, int division, NetworkConnectionToClient connection)
        {
            MatchPlayer player = new MatchPlayer()
                {PlayFabId = playFabId, PlayerCards = new PlayerCards(cards.CardsIdDeck), Name = name, Division = division, Connection = connection};
            MatchDetails.Players.Add(player);
        }

        public void AddBot(PlayerCards cards, string name)
        {
            MatchBot bot = new MatchBot(this) {PlayerCards = new PlayerCards(cards.CardsIdDeck), Name = name};
            MatchDetails.Players.Add(bot);
        }

        public void Start()
        {
            Debug.Log("Match starting");
            matchState = MatchState.Game;
            MatchDetails.Division = MatchDetails.Players.Select(p => p.Division).Min();
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
                MatchDetails.LevelInfo = new ();
            SendOutMatchDetails();
        }

        public void HandlePlayCardRequest(PlayerData playerData, CardData cardData)
        {
            if (matchState != MatchState.Game || MatchDetails.Players.Count <= 1)
                return;
            
            Debug.Log("Playing card");
            if (discardNextTurn || matchState != MatchState.Game)
                return;

            MatchPlayer matchPlayer = MatchDetails.CurrentPlayer;
            if (matchPlayer == null || matchPlayer.PlayFabId != playerData.PlayFabId)
                return;

            if (!TurnValidator.ValidateCardTurn(matchPlayer, Guid.Parse(cardData.Id), cardData))
            {
                HandleImpossibleMove();
                return;
            }

            PlayCard(cardData);
            
            MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();
            saveNextTurn = false;

            NotifyClientsAboutPlayedCard(cardData, playerData.PlayFabId);
            SendOutMatchDetails();
            ValidateGameState();
        }

        private void PlayCard(CardData cardData)
        {
            if (matchState != MatchState.Game || MatchDetails.Players.Count <= 1)
                return;

            foreach (var res in cardData.Cost)
            {
                MatchDetails.CurrentPlayer?.Castle.GetResource(res.Name).RemoveResource(res.Value);
            }
            
            saveNextTurn = cardData.SaveTurn;
            foreach (var effect in cardData.Effects)
            {
                if (effect is DiscardEffect)
                {
                    discardNextTurn = true;
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
                return;

            MatchPlayer matchPlayer = MatchDetails.CurrentPlayer;
            if (matchPlayer == null || matchPlayer.PlayFabId != playerData.PlayFabId)
                return;

            if (!TurnValidator.ValidateCardInHand(matchPlayer, Guid.Parse(cardData.Id)) || cardData.NonDiscard)
            {
                HandleImpossibleMove();
                return;
            }

            if (discardNextTurn)
            {
                discardNextTurn = false;
                MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
                MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.RequestDiscard);
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.Draft);
                SendOutMatchDetails();
                return;
            }
            
            MatchDetails.CurrentPlayer?.PlayerCards!.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer?.PlayerCards!.FillHand();

            PassTheMove();

            //NotifyClientsAboutCardDiscard(cardData, pla);
            SendOutMatchDetails();
            ValidateGameState();
        }

        public void PassTheMove(bool force = false)
        {
            if (saveNextTurn && !force)
                return;
            
            saveNextTurn = false;
            if(force)
                discardNextTurn = false;
            
            MatchDetails.Turn++;

            if (MatchDetails.Turn != 0 && MatchDetails.Turn % 2 == 0)
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

        private void HandleImpossibleMove()
        {
            if (matchState != MatchState.Game)
                return;

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
        }

        private void NotifyClientsAboutPlayedCard(CardData cardData, string playerId)
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
                DamagePlayer(player, MatchDetails.Fatigue.Damage, false);
                player.Connection?.Send(new FatigueDto()
                {
                    PlayerId = Guid.Empty,
                    Damage = MatchDetails.Fatigue.Damage
                });
            }

            ValidateGameState();
        }

        private void DamagePlayer(MatchPlayer player, int damage, bool spread = true)
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

            ValidateGameState();
        }

        public void Stop()
        {
            matchState = MatchState.Ended;
            MatchServerController.RemoveMatch(MatchId);
        }

        private void ValidateGameState()
        {
            List<MatchPlayer> winPlayers = new();
            List<MatchPlayer> losePlayers = new();
            foreach (var player in MatchDetails.Players)
            {
                Debug.Log($"Validation player info:\nMaxHp: {player.Castle.Tower.MaxHealth} Hp: {player.Castle.Tower.Health}");
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
                if (MatchDetails.LevelInfo != null && MatchDetails.MapProgress != null)
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