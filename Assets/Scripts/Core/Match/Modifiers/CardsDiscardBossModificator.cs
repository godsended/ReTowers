using System;
using System.Linq;
using Core.Cards;
using Core.Match.Server;

namespace Core.Match.Modifiers
{
    public class CardsDiscardBossModificator : IMatchModificator
    {
        private readonly MatchServer match;

        private readonly MatchPlayer player;

        private readonly Random random;

        private readonly double probability;
        
        private readonly Guid cardGuid = Guid.Parse("2e3efd93-628a-48de-b4b2-12ca1d4473c5");
        
        /// <summary>
        /// Must be created before bot!
        /// </summary>
        /// <param name="match">Current match</param>
        /// <param name="player">Player</param>
        /// <param name="probability">Probability, from 0 to 1</param>
        public CardsDiscardBossModificator(MatchServer match, MatchPlayer player, double probability)
        {
            this.match = match;
            this.player = player;
            this.probability = probability;
            this.random = new Random(match.GetHashCode() + player.GetHashCode());
            match.OnTurnPassed += OnTurnPassed;
        }

        private void OnTurnPassed(object sender, EventArgs e)
        {
            if (match.MatchDetails.CurrentPlayer == player) return;
            
            var rand = random.NextDouble();
            rand -= (int) rand;
            if (rand < probability)
                Discard();
        }

        private void Discard()
        {
            var count = random.Next() % 5 + 1;
            for (int i = 0; i < count; i++)
            {
                player.PlayerCards?.RemoveCardFromHand(player.PlayerCards.CardsIdHand.First());
            }
            player.PlayerCards?.FillHand();
            match.PassTheMove(true);
            match.NotifyClientsAboutPlayedCard(LibraryCards.GetCard(cardGuid), "");
            match.SendOutMatchDetails();
        }
    }
}