using System;
using Core.Cards;
using Core.Server;

namespace Core.Utils
{
    public static class TurnValidator
    {
        public static bool ValidateCardTurn(PlayerData player, Guid cardId)
        {
            return player.Cards.CardsIdHand.Contains(cardId);
        }
    }
}