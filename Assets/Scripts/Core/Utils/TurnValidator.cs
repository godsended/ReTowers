using System;
using System.Linq;
using Core.Cards;
using Core.Server;

namespace Core.Utils
{
    public static class TurnValidator
    {
        public static bool ValidateCardTurn(PlayerData player, Guid cardId, CardData cardData)
        {
            return ValidateCardInHand(player, cardId) && ValidateResourcesAvailability(player, cardData);
        }

        public static bool ValidateCardInHand(PlayerData player, Guid cardId)
        {
            return player.Cards.CardsIdHand.Contains(cardId);
        }

        public static bool ValidateResourcesAvailability(PlayerData player, CardData cardData)
        {
            // foreach (var resource in cardData.Cost)
            // {
            //     if (player.Castle.Resources.FirstOrDefault(c => c.Name == resource.Name && c.Value >= resource.Value) == null)
            //         return false;
            // }

            return true;
        }
    }
}