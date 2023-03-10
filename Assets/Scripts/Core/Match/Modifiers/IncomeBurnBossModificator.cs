using System;
using Core.Cards;
using Core.Match.Server;

namespace Core.Match.Modifiers
{
    public class IncomeBurnBossModificator : IMatchModificator
    {
        private readonly MatchServer match;

        private readonly MatchPlayer player;

        private readonly Random random;

        private readonly double probability;

        private readonly Guid cardGuid = Guid.Parse("7dc2b0fb-88a1-453c-8c87-724a66c0619a");
        
        /// <summary>
        /// Must be created before bot!
        /// </summary>
        /// <param name="match">Current match</param>
        /// <param name="player">Player</param>
        /// <param name="probability">Probability, from 0 to 1</param>
        public IncomeBurnBossModificator(MatchServer match, MatchPlayer player, double probability)
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
                Burn();
        }

        private void Burn()
        {
            player.Castle.Resources[random.Next(0, player.Castle.Resources.Count)].Income = 1;
            match.PassTheMove(true);
            match.NotifyClientsAboutPlayedCard(LibraryCards.GetCard(cardGuid), "");
            match.SendOutMatchDetails();
        }
    }
}