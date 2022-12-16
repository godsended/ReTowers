using Core.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab.ClientModels;

namespace Core.Cards
{
    public class LibraryCards : MonoBehaviour
    {
        private static LibraryCards instance;

        public List<CardData> CardDatas;
        public List<CardData> DeckDatas;

        private bool isCustomDeck;
        private List<CardData> customDeckDatas;
        private bool isShuffle;

        private void Start()
        {
            try
            {
                instance = this;

                var customCardNames = Configurator.data["CustomDecks"]["customDecks"]
                    .Replace(" ", string.Empty)
                    .Split('|')
                    .ToList();

                isCustomDeck = bool.Parse(Configurator.data["CustomDecks"]["isCustomDeck"]);
                isShuffle = bool.Parse(Configurator.data["CustomDecks"]["doShuffle"]);
                customDeckDatas = new List<CardData>();

                foreach (var cardName in customCardNames)
                {
                    var card = CardDatas.FirstOrDefault(c => c.Name == cardName);

                    if (card != null)
                        customDeckDatas.Add(card);
                }
            }
            catch (Exception e)
            {
            }
        }

        public static CardData GetCard(Guid id)
        {
            return instance.CardDatas.FirstOrDefault(c => c.Id == id.ToString());
        }

        public static List<CardData> GetPlayerCards()
        {
            List<CardData> playerCards = new List<CardData>();

            if (instance.isCustomDeck)
            {
                instance.customDeckDatas.ForEach(c => playerCards.Add(c));
            }
            else
            {
                instance.CardDatas.ForEach(c => playerCards.Add(c));
            }

            if (instance.isShuffle)
            {
                playerCards.Shuffle();
            }

            return playerCards;
        }

        public static List<CardData> GetPlayerDeckCards()
        {
            return instance.DeckDatas;
        }

        public static void AddCardInDeck(CardData cardData) 
        {
            instance.DeckDatas.Add(cardData);
            cardData.InDeck = true;
            UpdateCardsInDataBase();
        }

        public static void RemoveCardFromDeck(CardData cardData)
        {
            instance.DeckDatas.Remove(cardData);
            cardData.InDeck = false;
            UpdateCardsInDataBase();
        }

        public static void UpdateCardsInDataBase() 
        {
            List<CardJson> cards = new List<CardJson>();

            foreach (var item in instance.CardDatas)
            {
                cards.Add(new CardJson(item.Id, item.Count, item.InDeck));
            }

            PlayfabCards.SaveCards(cards);
        }

        public static void GetCardsFromDataBase()
        {
            PlayfabCards.GetPlayerCards(OnPlayerCardsDataRecieved);
        }

        static void OnPlayerCardsDataRecieved(GetUserDataResult result)
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                List<CardJson> cards = JsonConvert.DeserializeObject<List<CardJson>>(result.Data["PlayerCards"].Value);
                if (cards.Count == instance.CardDatas.Count)
                {
                    foreach (var item in cards)
                    {
                        for (int i = 0; i < instance.CardDatas.Count; i++)
                        {
                            if (instance.CardDatas[i].Id == item.Id)
                            {
                                instance.CardDatas[i].InDeck = item.InDeck;
                                instance.CardDatas[i].Count = item.Count;
                                break;
                            }
                        }
                    }
                    return;
                }
            }

            foreach (var item in instance.CardDatas)
            {
                item.Count = 0;
                item.InDeck = false;
                if (item.Rang == 0 && !string.IsNullOrEmpty(item.Type))
                {
                    item.Count = 1;
                    item.InDeck = true;
                }
            }
            UpdateCardsInDataBase();
        }
    }
}     
      
      
      