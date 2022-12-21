using Core.Cards;
using Core.Castle;
using Core.Contracts;
using Core.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;

namespace Core.Server
{
    /// <summary>
    /// Match entity
    /// Responsible for match logic
    /// </summary>
    public class Match
    {
        public Guid Id { get; private set; }
        public Dictionary<PlayerData, bool> Players { get; private set; }
        public PlayerData CurrentPlayerTurn { get; private set; }
        public int TurnTime { get; private set; }
        public int PrepareTime { get; private set; }
        
        public int MatchDivision { get; set; }

        private Coroutine turnCoroutine;
        private int _numberTurn;
        private int _numberTurnForFatigue;
        private int _damageFatigue;
        private IGameLogger _gameLogger;

        private List<Guid> _botDecks;
        private PlayerData bot;

        public Match(PlayerData player, IGameLogger gameLogger)
        {
            Init(gameLogger);

            InitPlayer(player);

            CurrentPlayerTurn = Players.FirstOrDefault().Key;

            _botDecks = CurrentPlayerTurn.Cards.CardsIdDeck;

            int[] lvl = new int[5];

            Debug.Log(_botDecks.Count);

            for (int i = 0; i < _botDecks.Count; i++)
            {
                CardData card = LibraryCards.GetCard(_botDecks[i]);
            
                if (card.Rang != 0)
                {
                    foreach (var cardData in LibraryCards.GetPlayerCards())
                    {
                        if (card.Type == cardData.Type && cardData.Rang == 0)
                        {
                            _botDecks[i] = Guid.Parse(cardData.Id);
                            lvl[card.Rang]++;
                            break;
                        }
                    }
                }
            }



            for (int i = 0; i < lvl.Length; i++)
            {
                for (int r = 0; r < lvl[i];)
                {
                    int numberCard = 0;
                    System.Random rand = new System.Random();
                    numberCard = rand.Next(0, _botDecks.Count);

                    CardData card = LibraryCards.GetCard(_botDecks[numberCard]);


                    if (card.Rang == 0)
                    {
                        foreach (var cardData in LibraryCards.GetPlayerCards())
                        {
                            if (card.Type == cardData.Type && cardData.Rang == i)
                            {
                                _botDecks[numberCard] = Guid.Parse(cardData.Id);

                                r++;
                                break;
                            }
                        }
                    }
                }
            }
            bot = new PlayerData();
            bot.Castle = new BlankCastleCreator().CreateCastle();

            _gameLogger.Log($"Create match {Id}!", LogTypeMessage.Info);
        }

        public Match(List<PlayerData> players, IGameLogger gameLogger)
        {
            Init(gameLogger);

            players.ForEach(InitPlayer);
            
            MatchDivision = players.First().Division;

            foreach (var player in players)
            {
                if (player.Division < MatchDivision)
                    MatchDivision = player.Division;
            }

            CurrentPlayerTurn = Players.FirstOrDefault().Key;

            _gameLogger.Log($"Create match {Id}!", LogTypeMessage.Info);
        }

        private void Init(IGameLogger gameLogger)
        {
            Id = Guid.NewGuid();
            Players = new Dictionary<PlayerData, bool>();
            TurnTime = int.Parse(Configurator.data["BattleConfiguration"]["turnTime"]);
            PrepareTime = 5;
            _gameLogger = gameLogger;
        }

        private void InitPlayer(PlayerData player)
        {
            player.CurrentMatch = this;
            player.Castle = new DivisionCastleCreator(MatchDivision).CreateCastle();
            player.Cards = new PlayerCards(player.Cards.CardsIdDeck, player.Connection);

            Players.Add(player, false);
        }

        public virtual void NextTurn()
        {
            try
            {
                CurrentPlayerTurn.Connection.Send(new RequestMatchDto
                {
                    RequestType = MatchRequestType.StartTurn
                });


                if (_numberTurn >= _numberTurnForFatigue - 2 && _numberTurn % 2 == 1)
                {
                    foreach (PlayerData player in Players.Keys)
                    {
                        if (player.Castle.Wall.Health > 0)
                            player.Castle.Wall.Damage(_damageFatigue);
                        else
                            player.Castle.Tower.Damage(_damageFatigue);
                    }
                
                    if (_damageFatigue < int.Parse(Configurator.data["BattleConfiguration"]["fatigueLimit"]))
                        _damageFatigue++;
                }

                PlayerData playerWin = CheckPlayerWin();

                if (playerWin == null)
                {
                    if (turnCoroutine != null)
                        MatchServerController.instance.StopCoroutine(turnCoroutine);

                    if (_numberTurn % 2 == 0)
                    {
                        foreach (PlayerData player in Players.Keys)
                            foreach (Resource resource in player.Castle.Resources)
                                resource.AddResource(resource.Income);
                    }

                    turnCoroutine = MatchServerController.instance.StartCoroutine(TurnTimer());
                    _numberTurn++;

                    _gameLogger.Log($"Match [{Id}]: Player turn - [{CurrentPlayerTurn.Id}]!", LogTypeMessage.Low);
                }
                else
                {
                    bool isDraw = playerWin.Castle.Tower.Health <= 0;

                    if (isDraw)
                    {
                        _gameLogger.Log($"Match [{Id}] stop: Draw!", LogTypeMessage.Info);

                        playerWin.Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.DrawMatch
                        });

                        Players.Keys.FirstOrDefault(p => p != playerWin).Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.DrawMatch
                        });
                    }
                    else
                    {
                        _gameLogger.Log($"Match [{Id}] stop: Player win - [{playerWin.Id}]", LogTypeMessage.Info);

                        playerWin.Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.WinMatch
                        });

                        Players.Keys.FirstOrDefault(p => p != playerWin).Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.LoseMatch
                        });
                    }

                    Stop();
                }
            }
            catch (Exception e)
            {
                _gameLogger.Log($"Match [{Id}] stop: {e}", LogTypeMessage.Warning);

                Stop();
            }
        }

        public IEnumerator BotTurn() 
        {
            yield return new WaitForSeconds(2f);

            //Debug.Log(bot.Castle.Resources[0].Value + " " + bot.Castle.Resources[1].Value + " " + bot.Castle.Resources[2].Value);
            Guid cardGuid = Guid.Empty;

            for (int i = 0; i < 5; i++)
            {
                Guid randomCardGuid = _botDecks[UnityEngine.Random.Range(0, _botDecks.Count)];
                CardData card = LibraryCards.GetCard(randomCardGuid);
                switch (card.Cost[0].Name)
                {
                    case "Resource_1":
                        if (bot.Castle.Resources[0].Value >= card.Cost[0].Value)
                        {
                            cardGuid = randomCardGuid;
                            bot.Castle.Resources[0].RemoveResource(card.Cost[0].Value);
                        }
                        break;
                    case "Resource_2":
                        if (bot.Castle.Resources[1].Value >= card.Cost[0].Value)
                        {
                            cardGuid = randomCardGuid;
                            bot.Castle.Resources[1].RemoveResource(card.Cost[0].Value);
                        }
                        break;
                    case "Resource_3":
                        if (bot.Castle.Resources[2].Value >= card.Cost[0].Value)
                        {
                            cardGuid = randomCardGuid;
                            bot.Castle.Resources[2].RemoveResource(card.Cost[0].Value);
                        }
                        break;
                }
                if (cardGuid != Guid.Empty) { break; }
            }

            CardData playedCard = null;

            if (cardGuid != Guid.Empty)
            {
                playedCard = LibraryCards.GetCard(cardGuid);

                PlayCard(new RequestCardDto
                {
                    AccountId = Guid.Empty,
                    ActionType = CardActionType.RequestPlay,
                    CardId = cardGuid
                });
            }

            foreach (Resource resource in bot.Castle.Resources)
                resource.AddResource(resource.Income);

            if(cardGuid == Guid.Empty) 
            {
                yield return new WaitForSeconds(1.5f);
                NextTurn();
            }
            else if(!playedCard.SaveTurn) 
            {
                yield return new WaitForSeconds(1.5f);
                NextTurn();
            }
        }

        public virtual void PlayCard(RequestCardDto requestCardDto)
        {
            PlayerData sendPlayer = Players.Keys.FirstOrDefault(p => p.Id == requestCardDto.AccountId);
            PlayerData targetPlayer = Players.Keys.FirstOrDefault(p => p.Id != requestCardDto.AccountId);
            CardData card = LibraryCards.GetCard(requestCardDto.CardId);

            if (card)
            {
                if(Players.Count == 1) 
                {
                    if (targetPlayer == null)
                    {
                        if (!CurrentPlayerTurn.Cards.CardsIdHand.Contains(requestCardDto.CardId))
                            return;
                        
                        CurrentPlayerTurn.Cards.RemoveCardFromHand(requestCardDto.CardId);
                        CurrentPlayerTurn.Cards.ShuffleCard(requestCardDto.CardId);
                        CurrentPlayerTurn.Cards.GetAndTakeNearestCard();

                        card.Effects.ForEach(e => e.Execute(CurrentPlayerTurn, bot));
                    }
                    else
                    {
                        card.Effects.ForEach(e => e.Execute(bot, CurrentPlayerTurn));
                        if (card.SaveTurn) 
                        {
                            MatchServerController.instance.StartBotTurn(this);
                        }
                    }
                }
                else if (sendPlayer != null)
                {
                    if (!TurnValidator.ValidateCardTurn(sendPlayer, requestCardDto.CardId))
                        return;

                    sendPlayer.Cards.RemoveCardFromHand(requestCardDto.CardId);
                    sendPlayer.Cards.ShuffleCard(requestCardDto.CardId);
                    sendPlayer.Cards.GetAndTakeNearestCard();
                    card.Effects.ForEach(e => e.Execute(sendPlayer, targetPlayer));
                }
                else return;

                //ТУТ У НАС БУДЕТ ОТПРАВКА ТЕКУЩЕЙ МОДЕЛИ ВСЕМ БОЙЦАМ
                foreach (PlayerData player in Players.Keys)
                {

                    player.Connection.Send(new RequestCardDto
                    {
                        AccountId = requestCardDto.AccountId,
                        CardId = requestCardDto.CardId,
                        ActionType = (player.Id == requestCardDto.AccountId)
                            ? CardActionType.YouPlayed
                            : CardActionType.EnemyPlayed
                    });
                }

                CheckEndMatch();
            }
            else
            {
                _gameLogger.Log($"Match [{Id}] error play card: card - [{card}]" +
                    $", target Player - [{targetPlayer}]", LogTypeMessage.Warning);
            }
        }

        public virtual void DiscardCard(RequestCardDto requestCardDto)
        {
            PlayerData sendPlayer = Players.Keys.FirstOrDefault(p => p.Id == requestCardDto.AccountId);
            CardData card = LibraryCards.GetCard(requestCardDto.CardId);

            if (sendPlayer == null || card == null || !TurnValidator.ValidateCardTurn(sendPlayer, requestCardDto.CardId)) 
                return;

            if (!sendPlayer.Cards.CardsIdHand.Contains(requestCardDto.CardId)) return;
            
            sendPlayer.Cards.RemoveCardFromHand(requestCardDto.CardId);
            sendPlayer.Cards.ShuffleCard(requestCardDto.CardId);
            sendPlayer.Cards.GetAndTakeNearestCard();
        }

        public void CheckEndMatch()
        {
            PlayerData playerWin = CheckPlayerWin();

            if (playerWin != null)
            {
                _gameLogger.Log($"Match [{Id}] stop: Player win - [{playerWin.Id}]", LogTypeMessage.Info);

                try
                {
                    playerWin.Connection.Send(new RequestMatchDto
                    {
                        RequestType = MatchRequestType.EndTurn
                    });

                    Players.Keys.FirstOrDefault(p => p != playerWin).Connection.Send(new RequestMatchDto
                    {
                        RequestType = MatchRequestType.EndTurn
                    });

                    bool isDraw = playerWin.Castle.Tower.Health <= 0;

                    if (isDraw)
                    {
                        playerWin.Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.DrawMatch
                        });

                        Players.Keys.FirstOrDefault(p => p != playerWin).Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.DrawMatch
                        });
                    }
                    else
                    {
                        playerWin.Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.WinMatch
                        });

                        Players.Keys.FirstOrDefault(p => p != playerWin).Connection.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.LoseMatch
                        });
                    }
                }
                catch (Exception e)
                {
                    _gameLogger.Log($"Match [{Id}] error: {e}", LogTypeMessage.Warning);
                }
                finally
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// Start this match
        /// </summary>
        public void Start()
        {
            _numberTurn = 0;
            _numberTurnForFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]);
            _damageFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueDamageStart"]);

            foreach (PlayerData playerData in Players.Keys)
                MatchServerController.instance.StartCoroutine(playerData.Cards.FillHand());

            turnCoroutine = MatchServerController.instance.StartCoroutine(TurnTimer());

            _gameLogger.Log($"Match [{Id}] start!", LogTypeMessage.Info);
        }

        /// <summary>
        /// Stop this match
        /// </summary>
        public void Stop()
        {
            foreach (PlayerData playerData in Players.Keys)
            {
                if (playerData.Connection != null)
                {
                    playerData.Connection.Send(new RequestMatchDto
                    {
                        AccountId = playerData.Id,
                        RequestType = MatchRequestType.ExitMatch
                    });
                }

                playerData.CurrentMatch = null;
            }

            Players.Clear();

            if (turnCoroutine != null)
                MatchServerController.instance.StopCoroutine(turnCoroutine);

            turnCoroutine = null;

            _gameLogger.Log($"Match [{Id}] stop!", LogTypeMessage.Info);

            MatchServerController.RemoveMatch(Id);
        }

        /// <summary>
        /// Set player as ready
        /// </summary>
        public void PlayerReady(Guid playerId)
        {
            PlayerData readyPlayer = Players.Keys.FirstOrDefault(p => p.Id == playerId);

            if (readyPlayer != null)
            {
                Players[readyPlayer] = true;

                _gameLogger.Log($"{readyPlayer.Name} [{readyPlayer.Id}] ready for match [{Id}]!", LogTypeMessage.Low);

                CheckForStartMatch();
            }
            else
            {
                _gameLogger.Log($"{readyPlayer.Name} [{readyPlayer.Id}] not found in match [{Id}]!", LogTypeMessage.Warning);
            }
        }

        public void SetDebugInfo()
        {
            DebugManager.AddLineDebugText("T1: " + Players.Keys.First().Castle.Tower.Health.ToString(), "t1");
            DebugManager.AddLineDebugText("W1: " + Players.Keys.First().Castle.Wall.Health.ToString(), "w1");
            DebugManager.AddLineDebugText("T2: " + Players.Keys.Last().Castle.Tower.Health.ToString(), "t2");
            DebugManager.AddLineDebugText("W2: " + Players.Keys.Last().Castle.Wall.Health.ToString(), "w2");
            DebugManager.AddLineDebugText("Turn: " + _numberTurn, "t");
            DebugManager.AddLineDebugText("FatigueDamage: " + _damageFatigue, "fd");
        }

        private void CheckForStartMatch()
        {
            foreach (var player in Players)
                if (player.Value == false)
                    return;

            Start();
        }

        private PlayerData CheckPlayerWin()
        {
            foreach (var player in Players.Keys)
                if (player.Castle.Tower.Health <= 0)
                    return Players.Keys.FirstOrDefault(p => p != player);

            foreach (var player in Players.Keys)
                if (player.Castle.Tower.Health >= player.Castle.Tower.MaxHealth)
                    return Players.Keys.FirstOrDefault(p => p == player);

            return null;
        }

        private IEnumerator TurnTimer()
        {
            yield return new WaitForSeconds(TurnTime);

            CurrentPlayerTurn.Connection.Send(new RequestMatchDto
            {
                RequestType = MatchRequestType.EndTurn
            });

            CheckBotMatch();
        }

        public IEnumerator WaitNextTurn()
        {
            CurrentPlayerTurn.Connection.Send(new RequestMatchDto
            {
                RequestType = MatchRequestType.EndTurn
            });

            CheckBotMatch();

            yield break;
        }
        private void CheckBotMatch()
        {
            if (Players.Count == 2)
            {
                CurrentPlayerTurn = Players.Keys.FirstOrDefault(p => p != CurrentPlayerTurn);

                NextTurn();
            }
            else
            {
                _numberTurn++;
                MatchServerController.instance.StartBotTurn(this);
            }
        }
    }
}