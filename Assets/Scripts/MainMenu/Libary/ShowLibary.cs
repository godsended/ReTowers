using UnityEngine;
using System.Collections.Generic;
using Core.Cards;
using UnityEngine.UI;
using TMPro;
using Core;

namespace MainMenu.Libary
{
    public class ShowLibary : MonoBehaviour
    {
        private static ShowLibary instance;

        [SerializeField] private TextMeshProUGUI cardsCount;

        [SerializeField] private GameObject content;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private CardMenu cardMenu;

        [SerializeField] private TextMeshProUGUI curDiv;
        [SerializeField] private TextMeshProUGUI newDiv;
        [SerializeField] private Slider slider;
        private float weight = 0;

        private List<CardUI> cardUIs = new List<CardUI>();
        private ResoursType resoursType = new ResoursType();
        private int RareType = -1;


        void Start()
        {
            instance = this;
            List<CardData> spawnCard = new List<CardData>();
            foreach(var card in LibraryCards.GetPlayerCards()) 
            {
                if(card.Count == 0) { continue; }
                bool add = true;
                for (int i = 0; i < spawnCard.Count; i++)
                {
                    if (spawnCard[i].Type == card.Type)
                    {
                        if (card.InDeck)
                        {
                            spawnCard[i] = card;
                        }
                        add = false;
                        break;
                    }
                }
                if (add) { 
                    spawnCard.Add(card);}
            }

            cardsCount.SetText("102 / 102");

            foreach (var card in spawnCard) 
            {
                GameObject spawnedCard = Instantiate(cardPrefab, content.transform);
                CardUI cardUI = spawnedCard.GetComponent<CardUI>();
                cardUIs.Add(cardUI);

                weight = DivisionCalculator.CalculateMass(spawnCard.ToArray());

                cardUI.SetCard(card, cardMenu);
            }

            UpdateDisplayRang();
        }

        public static void UpdateDisplayRang() 
        {
            int div = DivisionCalculator.SpotDivision(instance.weight);

            instance.curDiv.text = div.ToString();
            instance.newDiv.text = (div + 1).ToString();

            instance.slider.minValue = 0;
            instance.slider.maxValue = 1250;

            if (div == 10) 
            {
                instance.newDiv.text = "max";
                instance.slider.minValue = 2850;
                instance.slider.maxValue = 3000;
            }
            else if(div == 9) 
            {
                instance.slider.minValue = 2650;
                instance.slider.maxValue = 2850;
            }
            else if (div == 8)
            {
                instance.slider.minValue = 2450;
                instance.slider.maxValue = 2650;
            }
            else if (div == 7)
            {
                instance.slider.minValue = 2250;
                instance.slider.maxValue = 2450;
            }
            else if (div == 6)
            {
                instance.slider.minValue = 2050;
                instance.slider.maxValue = 2250;
            }
            else if (div == 5)
            {
                instance.slider.minValue = 1850;
                instance.slider.maxValue = 2050;
            }
            else if (div == 4)
            {
                instance.slider.minValue = 1650;
                instance.slider.maxValue = 1850;
            }
            else if (div == 3)
            {
                instance.slider.minValue = 1550;
                instance.slider.maxValue = 1650;
            }
            else if (div == 2)
            {
                instance.slider.minValue = 1250;
                instance.slider.maxValue = 1550;
            }


            instance.slider.value = instance.weight;
        }

        CardData[] datas = new CardData[1];
        public static void RemvoeAtWeight(CardData card)
        {
            instance.weight = DivisionCalculator.RemoveMass(card, instance.weight);

            UpdateDisplayRang();
        }

        public static void AddToWeight(CardData card)
        {
            instance.weight = DivisionCalculator.AddMass(card, instance.weight);

            UpdateDisplayRang();
        }

        private void ShowWithFiltr() 
        {
            foreach (var item in cardUIs)
            {
                item.gameObject.SetActive(true);
            }

            foreach (var item in cardUIs)
            {
                if (resoursType == ResoursType.None)
                    break;
                if(item.GetCardData().Cost[0].Name != resoursType.ToString()) 
                {
                    item.gameObject.SetActive(false);
                }
            }

            foreach (var item in cardUIs)
            {
                if (RareType == -1)
                    break;
                if (item.GetCardData().Rang != RareType)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        public void SetResoursesType(ResoursType type) 
        {
            resoursType = type;
            ShowWithFiltr();
        }

        public void SetLevelFiltr(int lvl)
        {
            RareType = lvl;
            ShowWithFiltr();
        }
    }
}
