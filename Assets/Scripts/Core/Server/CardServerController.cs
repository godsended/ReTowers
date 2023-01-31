using Core.Cards;
using UnityEngine;
using Mirror;
using Core.Contracts;
using Newtonsoft.Json;

namespace Core.Server
{
    public class CardServerController : MonoBehaviour
    {
        private static CardServerController instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            NetworkServer.RegisterHandler<RequestCardDto>(HandleCardAction, false);
        }

        private void HandleCardAction(NetworkConnectionToClient connection, RequestCardDto requestCardDto)
        {
            Debug.Log("Card request accepted. Dto:\n" + JsonConvert.SerializeObject(requestCardDto));
            PlayerData player = MainServer.GetPlayerData(requestCardDto.AccountId);
            if (player == null)
            {
                Debug.Log("Card request player is NULL");
                return;
            }

            Debug.Log(JsonConvert.SerializeObject(player));
            
            CardData cardData = LibraryCards.GetCard(requestCardDto.CardId);
            if (cardData == null)
            {
                Debug.Log("Card request card data is NULL");
                return;
            }

            switch (requestCardDto.ActionType)
            {
                case CardActionType.RequestPlay:
                    player.CurrentMatch?.HandlePlayCardRequest(player, cardData);
                    break;
                case CardActionType.RequestDiscard:
                    player.CurrentMatch?.HandleDiscardCardRequest(player, cardData);
                    break;
            }
        }
    }
}