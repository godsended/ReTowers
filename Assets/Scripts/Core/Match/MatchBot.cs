using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Cards;
using Core.Contracts;
using Core.Match.Server;
using Core.Server;
using Core.Utils;

namespace Core.Match
{
    public class MatchBot : MatchPlayer, IMatchBot
    {
        private readonly MatchServer server;
        
        public MatchBot(MatchServer server)
        {
            PlayFabId = "";
            this.server = server;
            server.OnTurnPassed += ServerOnOnTurnPassed;
        }

        private void ServerOnOnTurnPassed(object sender, EventArgs e)
        {
            if (server.MatchDetails.CurrentPlayer == this)
            {
                Task.Delay(2500).ContinueWith(_ => MatchServerController.instance.ConcurrentActions.Add(Turn));
            }
        }

        private void Turn()
        {
            if (!server.SaveNextTurn && server.DiscardNextTurn)
            {
                RequestPassTheMove();
                return;
            }
            
            CardData currCard = null;
            int maxSum = -1;
            foreach (var guid in PlayerCards.CardsIdHand)
            {
                CardData cardData = LibraryCards.GetCard(guid);
                if (!TurnValidator.ValidateCardTurn(this, guid, cardData)) 
                    continue;
                
                int totalCost = cardData.Cost.Sum(resource => resource.Value);
                if (currCard == null)
                {
                    currCard = cardData;
                    maxSum = totalCost;
                    continue;
                }

                if (totalCost <= maxSum)
                    continue;
                
                currCard = cardData;
                maxSum = totalCost;
            }

            if (currCard == null)
            {
                RequestPassTheMove();
                return;
            }

            PlayerData playerData = new() {PlayFabId = PlayFabId};
            server.HandlePlayCardRequest(playerData, currCard);
        }

        private void RequestPassTheMove()
        {
            server.PassTheMove(true);
            server.SendOutMatchDetails();
        }
    }
}