using Core.Cards;
using UnityEngine;
using Mirror;
using Core.Contracts;

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
            PlayerData player = MainServer.GetPlayerData(requestCardDto.AccountId);
            if (player == null)
                return;

            CardData cardData = LibraryCards.GetCard(requestCardDto.CardId);
            if (cardData == null)
                return;
            
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