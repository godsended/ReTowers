using System;
using System.Linq;
using Core.Cards;
using Core.Match;
using Core.Server;

namespace Core.Utils
{
    public static class TurnValidator
    {
        public static bool ValidateCardTurn(MatchPlayer player, Guid cardId, CardData cardData)
        {
            return ValidateCardInHand(player, cardId) && ValidateResourcesAvailability(player, cardData);
        }

        public static bool ValidateCardInHand(MatchPlayer player, Guid cardId)
        {
            return player.PlayerCards.CardsIdHand.Contains(cardId);
        }

        public static bool ValidateResourcesAvailability(MatchPlayer player, CardData cardData)
        {
            foreach (var resource in cardData.Cost)
            {
                if (player.Castle.Resources.FirstOrDefault(c => c.Name == resource.Name && c.Value >= resource.Value) == null)
                    return false;
            }

            return true;
        }
    }
}