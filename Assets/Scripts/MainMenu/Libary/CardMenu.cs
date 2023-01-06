using UnityEngine;
using Core.Cards;
using UnityEngine.UI;
using TMPro;

namespace MainMenu.Library {
    public class CardMenu : MonoBehaviour
    {
        [SerializeField] private Image bigCardImage;
        //[SerializeField] private Image doubleCardImage;
        [SerializeField] private Image[] cardsLevelImage;
        [SerializeField] private TextMeshProUGUI[] cardsCountText;

        private CardData _cardData;
        private CardUI _cardUI;
        public void SetCard(CardUI cardUI) 
        {
            _cardUI = cardUI;
            _cardData = _cardUI.GetCardData();

            int countCards = 0;
            foreach(var item in LibraryCards.GetPlayerCards()) 
            {
                if(item.Type == _cardData.Type) 
                {
                    cardsLevelImage[item.Rang].sprite = item.CardImage;
                    if (item.Count == 0)
                    {
                        cardsLevelImage[item.Rang].color = Color.gray;
                    }
                    else
                    {
                        cardsLevelImage[item.Rang].color = Color.white;
                        countCards++;
                    }
                    cardsCountText[item.Rang].SetText(item.Count.ToString());
                }
            }

            //if (countCards >= 2)
            //{
            //    doubleCardImage.gameObject.SetActive(true);
            //}
            //else 
            //{
            //    doubleCardImage.gameObject.SetActive(false);
            //}
            bigCardImage.sprite = _cardData.CardImage;
        }

        public void AddOrRemoveCardinDeck() 
        {
            if (_cardData.InDeck) 
            {
                RemoveCardFromDeck();
            }
            else 
            {
                AddCardInDeck();
            }
        }

        public void SetOtherCardLevel(int lvl) 
        {
            foreach (var item in LibraryCards.GetPlayerCards())
            {
                if(item.Type == _cardData.Type && item.Rang == lvl) 
                {
                    if(item.Count == 0) { return; }
                    RemoveCardFromDeck();
                    _cardData = item;
                    AddCardInDeck();
                    _cardUI.SetCard(_cardData, this);
                    bigCardImage.sprite = _cardData.CardImage;
                    return;
                }
            }  
        }

        private void AddCardInDeck() 
        {
            LibraryCards.AddCardInDeck(_cardData);
            ShowLibary.AddToWeight(_cardData);
        }
        private void RemoveCardFromDeck() 
        {
            ShowLibary.RemvoeAtWeight(_cardData);
            LibraryCards.RemoveCardFromDeck(_cardData);
        }
    }
}
