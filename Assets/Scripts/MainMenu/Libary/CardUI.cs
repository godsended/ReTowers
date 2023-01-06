using UnityEngine.UI;
using UnityEngine;
using Core.Cards;

namespace MainMenu.Library
{
    public class CardUI : MonoBehaviour
    {
        [SerializeField] private Image cardImage;
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
            cardImage.sprite = _curCardData.CardImage;

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
                    cardImage.sprite = item.CardImage;
                    UpdateCardUI();
                    break;
                }
            }

            menu.SetCard(this);
            LibraryCards.UpdateCardsInDataBase();
        }
    }
}
