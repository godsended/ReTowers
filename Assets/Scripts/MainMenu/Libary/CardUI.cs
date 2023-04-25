using UnityEngine.UI;
using UnityEngine;
using Core.Cards;

namespace MainMenu.Library
{
    public class CardUI : MonoBehaviour, ICardConfigurableObject
    {
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private Image cardForegroundImage;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text costText;
        [SerializeField] private CardMenu menu;
        [SerializeField] private GameObject isDoubleCard;

        private CardData _curCardData;

        public CardData GetCardData() 
        {
            return _curCardData;
        }

        public void SetCard(CardData newCardData, CardMenu cardMenu)
        {
            menu = cardMenu;

            _curCardData = newCardData;
            CardObjectsConfigurator.Configure(this, _curCardData);

            UpdateCardUI();
        }

        public void UpdateCardUI() 
        {
            int countCards = 0;
            foreach (var item in LibraryCards.GetPlayerCards())
            {
                if(item.Type == _curCardData.Type && item.Count > 0) 
                {
                    countCards++;
                }
            }
            if (countCards >= 2)
            {
                isDoubleCard.SetActive(true);
            }
        }

        public void OpenCardMenu()
        {
            menu.gameObject.SetActive(true);
            menu.SetCard(this);
        }

        public void UpCardLevel()
        {
            if (_curCardData.Rang >= 4)
                return;
            
            _curCardData.UpLevel();
            ShowLibary.RemvoeAtWeight(_curCardData);
            _curCardData.InDeck = false;
            foreach (var item in LibraryCards.GetPlayerCards())
            {
                if (item.Type == _curCardData.Type && item.Rang - 1 == _curCardData.Rang)
                {
                    _curCardData = item;
                    _curCardData.InDeck = true;
                    ShowLibary.AddToWeight(item);
                    CardObjectsConfigurator.Configure(this, _curCardData);
                    UpdateCardUI();
                    break;
                }
            }

            menu.SetCard(this);
            LibraryCards.UpdateCardsInDataBase();
        }

        public void SetBackgroundImageSprite(Sprite sprite)
        {
            cardBackgroundImage.sprite = sprite;
        }

        public void SetForegroundImageSprite(Sprite sprite)
        {
            cardForegroundImage.sprite = sprite;
        }

        public void SetTitle(string text)
        {
            titleText.text = text;
        }
        
        public void SetCostText(string text)
        {
            costText.text = text;
        }

        public void SetDescription(string description)
        {
            descriptionText.text = description;
        }

        public void SetBackgroundImageColor(Color color)
        {
            cardBackgroundImage.color = color;
        }
    }
}
