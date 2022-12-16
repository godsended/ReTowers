using UnityEngine;
using System.Collections.Generic;
using Core.Cards;
using UnityEngine.UI;

namespace Core.Client
{
    public class HistoryUI : MonoBehaviour
    {
        private static HistoryUI instance;

        public List<CardData> cardsYourHistory;
        public List<CardData> cardsEnemyHistory;

        public List<Image> imagesYourHistory;
        public List<Image> imagesEnemyHistory;

        public static void AddMyHistory(CardData card)
        {
            instance.cardsYourHistory.Insert(0, card);

            if (instance.cardsYourHistory.Count > 3)
                instance.cardsYourHistory.RemoveAt(4);

            for (int i = 0; i < instance.imagesYourHistory.Count; i++)
            {
                try
                {
                    instance.imagesYourHistory[i].color = Color.white;
                    instance.imagesYourHistory[i].sprite = instance.cardsYourHistory[i].CardImage;
                }
                catch
                {
                    instance.imagesYourHistory[i].color = Color.clear;
                }
            }
        }

        public static void AddEnemyHistory(CardData card)
        {
            instance.cardsEnemyHistory.Insert(0, card);

            if (instance.cardsEnemyHistory.Count > 3)
                instance.cardsEnemyHistory.RemoveAt(4);

            for (int i = 0; i < instance.imagesEnemyHistory.Count; i++)
            {
                try
                {
                    instance.imagesEnemyHistory[i].color = Color.white;
                    instance.imagesEnemyHistory[i].sprite = instance.cardsEnemyHistory[i].CardImage;
                }
                catch
                {
                    instance.imagesEnemyHistory[i].color = Color.clear;
                }
            }
        }
        private void Awake()
        {
            instance = this;
        }
    }
}
