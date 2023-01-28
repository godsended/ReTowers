using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cards;
using Core.Cards.Effects;
using Core.Contracts;
using Core.Map;
using Core.Map.Server;
using Core.Server;
using Core.Utils;
using Mirror;
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
                    player.PlayerCards.FillHand();
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
        }

        private void PlayCard(CardData cardData)
        {
            if (matchState != MatchState.Game)
                return;

            saveNextTurn = cardData.SaveTurn;
            foreach (var effect in cardData.Effects)
            {
                if (effect is DiscardEffect)
                {
                    discardNextTurn = true;
                }

                effect.Execute(MatchDetails.CurrentPlayer, MatchDetails.NextPlayer);
            }

            if (!saveNextTurn)
            {
                MatchDetails.Turn++;
                if (MatchDetails.Turn >= MatchDetails.FatigueTurn)
                {
                    DamageFatigue();
                    MatchDetails.Fatigue++;
                }
            }

            MatchDetails.CurrentPlayer.PlayerCards.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer.PlayerCards.FillHand();
            saveNextTurn = false;

            SendOutMatchDetails();
            ValidateGameState();
        }

        public void HandleDiscardCardRequest(PlayerData playerData, CardData cardData)
        {
            if (matchState != MatchState.Game)
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
                MatchDetails.CurrentPlayer.PlayerCards.RemoveCardFromHand(Guid.Parse(cardData.Id));
                MatchDetails.CurrentPlayer.PlayerCards.FillHand();
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.RequestDiscard);
                //NotifyClientsAboutPlayedCard(cardData, CardActionType.Draft);
                SendOutMatchDetails();
                return;
            }

            if (!saveNextTurn)
            {
                MatchDetails.Turn++;
                if (MatchDetails.Turn >= MatchDetails.FatigueTurn)
                {
                    DamageFatigue();
                    MatchDetails.Fatigue++;
                }
            }

            MatchDetails.CurrentPlayer.PlayerCards.RemoveCardFromHand(Guid.Parse(cardData.Id));
            MatchDetails.CurrentPlayer.PlayerCards.FillHand();
            saveNextTurn = false;

            //NotifyClientsAboutPlayedCard(cardData, CardActionType.RequestDiscard);
            //NotifyClientsAboutPlayedCard(cardData, CardActionType.Draft);
            SendOutMatchDetails();
            ValidateGameState();
        }

        private void HandleImpossibleMove()
        {
            if (matchState != MatchState.Game)
                return;

            SendOutMatchDetails();
        }

        private void SendOutMatchDetails()
        {
            if (matchState != MatchState.Game)
                return;

            foreach (var player in MatchDetails.Players)
            {
                player.Connection.Send(CreateMatchDetailsDto(player));
            }
        }

        private void NotifyClientsAboutPlayedCard(CardData cardData, CardActionType actionType)
        {
            if (matchState != MatchState.Game)
                return;

            foreach (var player in MatchDetails.Players)
            {
                RequestCardDto dto = new()
                {
                    AccountId = Guid.Parse(MatchDetails.CurrentPlayer.PlayFabId),
                    CardId = Guid.Parse(cardData.Id),
                    ActionType = actionType
                };
                player.Connection.Send(dto);
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
                IsYourTurn = MatchDetails.CurrentPlayer.PlayFabId == player.PlayFabId,
                CardsInHandIds = player?.PlayerCards?.CardsIdHand?.Select(c => c.ToString())?.ToArray(),
                LevelInfo = MatchDetails.LevelInfo
            };
        }

        private void DamageFatigue()
        {
            if (matchState != MatchState.Game)
                return;

            foreach (var player in MatchDetails.Players)
            {
                DamagePlayer(player, MatchDetails.Fatigue.Damage, false);
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
                if (player.Castle.Tower.Health >= player.Castle.Tower.MaxHealth)
                {
                    winPlayers.Add(player);
                }

                if (player.Castle.Tower.Health <= 0)
                {
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
                Stop();
        }

        private void SendGameResult(MatchPlayer player, bool isWin)
        {
            RequestMatchDto dto = new()
            {
                AccountId = Guid.Parse(player.PlayFabId),
                // ReSharper disable once Unity.NoNullPropagation
                LevelId = MatchDetails.LevelInfo?.LevelId ?? -1,
                RequestType = isWin ? MatchRequestType.WinMatch : MatchRequestType.LoseMatch
            };
            player.Connection.Send(dto);
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