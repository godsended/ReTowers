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
            MatchId = Guid.NewGuid();
            MatchDetails = isPve ? new PveMatchDetails() : new MatchDetails();
            this.isPve = isPve;
            MatchDetails.TurnTime = int.Parse(Configurator.data["BattleConfiguration"]["turnTime"]);
            MatchDetails.FatigueTurn = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]);
            MatchDetails.PrepareTime = 5;
        }

        public void AddPlayer(string playFabId, string name, PlayerCards cards)
        {
            MatchPlayer player = new MatchPlayer() {PlayFabId = playFabId, PlayerCards = cards, Name = name};
            MatchDetails.Players.Add(player);
        }

        public void AddBot(PlayerCards cards, string name)
        {
            MatchBot bot = new MatchBot(this) {PlayerCards = cards, Name = name};
            MatchDetails.Players.Add(bot);
        }

        public void Start()
        {
            matchState = MatchState.Game;
            foreach (var player in MatchDetails.Players)
            {
                player.Castle = new DivisionCastleCreator(MatchDetails.Division).CreateCastle();
            }

            MatchDetails.Fatigue = new Fatigue(MatchDetails.Division);
            MatchDetails.Turn = 0;
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
                if (isPve)
                    player.Connection.Send(CreatePveMatchDetailsDto(player));
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
                Players = MatchDetails.Players.Select(p => new MatchPlayerDto()
                {
                    Castle = p.Castle, Name = p.Name
                }).ToArray(),
                Fatigue = MatchDetails.Fatigue,
                IsYourTurn = MatchDetails.CurrentPlayer.PlayFabId == player.PlayFabId,
                CardsInHandIds = player.PlayerCards.CardsIdHand.Select(c => c.ToString()).ToArray()
            };
        }

        private PveMatchDetailsDto CreatePveMatchDetailsDto(MatchPlayer player)
        {
            return new()
            {
                Players = MatchDetails.Players.Select(p => new MatchPlayerDto()
                {
                    Castle = p.Castle, Name = p.Name
                }).ToArray(),
                Fatigue = MatchDetails.Fatigue,
                IsYourTurn = MatchDetails.CurrentPlayer.PlayFabId == player.PlayFabId,
                CardsInHandIds = player.PlayerCards.CardsIdHand.Select(c => c.ToString()).ToArray(),
                LevelInfo = (MatchDetails as PveMatchDetails)?.LevelInfo
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
            
            if(MatchDetails.Players.Count < 2)
                Stop();
        }

        private void SendGameResult(MatchPlayer player, bool isWin)
        {
            RequestMatchDto dto = new()
            {
                AccountId = Guid.Parse(player.PlayFabId),
                LevelId = (MatchDetails as PveMatchDetails)?.LevelInfo.LevelId ?? -1,
                RequestType = isWin ? MatchRequestType.WinMatch : MatchRequestType.LoseMatch
            };
            player.Connection.Send(dto);
        }

        private void ApplyAfterGameStatistics(MatchPlayer player, bool isWin)
        {
            if (isWin)
            {
                var pveDetails = MatchDetails as PveMatchDetails;
                if (pveDetails != null)
                {
                    ServerMapController.Instance.UpdateMapProgress(pveDetails.MapProgress, 
                        pveDetails.LevelInfo, player.PlayFabId);
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