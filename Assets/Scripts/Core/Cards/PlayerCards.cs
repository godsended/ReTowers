using Core.Contracts;
using Core.Logging;
using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;

namespace Core.Cards
{
    public class PlayerCards
    {
        public static readonly int CardsInHandLimit = 6;
        public List<Guid> CardsIdDeck { get; private set; }

        public List<Guid> CardsIdHand { get; private set; }

        private NetworkConnectionToClient _connection;
        private IGameLogger _gameLogger;

        public bool IsHandFilled => CardsIdHand.Count >= CardsInHandLimit;

        public PlayerCards(List<Guid> cards, bool removeIncollectibleCards = true)
        {
            CardsIdDeck = new();
            cards.ForEach(guid => CardsIdDeck.Add(guid));
            if (removeIncollectibleCards)
                CardsIdDeck.RemoveAll(c => !LibraryCards.GetCard(c).IsCollectible);
            CardsIdHand = new List<Guid>();
            ShuffleCards();
        }

        public PlayerCards(List<Guid> cards, NetworkConnectionToClient connection, bool removeIncollectibleCards = true)
        {
            CardsIdDeck = cards;
            if (removeIncollectibleCards)
                CardsIdDeck.RemoveAll(c => !LibraryCards.GetCard(c).IsCollectible);
            CardsIdHand = new List<Guid>();

            _connection = connection;

            _gameLogger = new ConsoleLogger(new List<LogTypeMessage>
            {
                LogTypeMessage.Error,
                LogTypeMessage.Warning,
                LogTypeMessage.Info,
                LogTypeMessage.Low
            });
        }

        public void RemoveCardFromHand(Guid cardId)
        {
            Debug.Log(CardsIdHand.Contains(cardId) + " " + cardId);
            Debug.Log(JsonConvert.SerializeObject(CardsIdHand));
            CardsIdHand.Remove(cardId);
            ShuffleCard(cardId);
        }

        public void FillHand()
        {
            for (int i = CardsIdHand.Count; i < CardsInHandLimit; i++)
            {
                GetAndTakeNearestCard();
            }
        }

        public void GetAndTakeNearestCard()
        {
            while (true)
            {
                Guid id = CardsIdDeck[0];
                CardsIdDeck.Remove(CardsIdDeck[0]);
                if (CardsIdHand.Contains(id))
                    continue;

                CardsIdHand.Add(id);
                break;
            }
        }

        public void ShuffleCard(Guid cardId, int maxIndex = 0)
        {
            CardsIdDeck.Add(cardId);
        }

        public void ShuffleCards()
        {
            for (int i = 0; i < CardsIdDeck.Count; i++)
            {
                int newPos = UnityEngine.Random.Range(0, CardsIdDeck.Count);
                (CardsIdDeck[i], CardsIdDeck[newPos]) = (CardsIdDeck[newPos], CardsIdDeck[i]);
            }
        }
    }
}