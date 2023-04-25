#if !UNITY_ANDROID

using System;
using Core.Cards;
using Core.Match.Server;

namespace Core.Match.Modifiers
{
    public class HealFreezeBossModificator : IMatchModificator
    {
        private readonly MatchServer match;

        private readonly MatchPlayer player;

        private readonly Random random;

        private readonly double probability;
        
        private readonly int cooldown = 3;
        private int cooldownState = 0;
        
        private readonly Guid cardGuid = Guid.Parse("a58150b9-c2cb-45f4-b32b-bdf9c48f740a");
        
        /// <summary>
        /// Must be created before bot!
        /// </summary>
        /// <param name="match">Current match</param>
        /// <param name="player">Player</param>
        /// <param name="probability">Probability, from 0 to 1</param>
        public HealFreezeBossModificator(MatchServer match, MatchPlayer player, double probability, int cooldown = 3)
        {
            this.match = match;
            this.player = player;
            this.probability = probability;
            this.random = new Random(match.GetHashCode() + player.GetHashCode());
            this.cooldown = cooldown;
            match.OnTurnPassed += OnTurnPassed;
        }

        private void OnTurnPassed(object sender, EventArgs e)
        {
            if (match.MatchDetails.CurrentPlayer == player) return;
            
            if (cooldownState > 0)
            {
                cooldownState--;
                return;
            }
            
            var rand = random.NextDouble();
            rand -= (int) rand;
            if (rand < probability)
            {
                FreezeHeal();
                cooldownState = cooldown;
            }
        }

        private void FreezeHeal()
        {
            player.Castle.Wall.MaxHealth = player.Castle.Wall.Health;
            match.PassTheMove(true);
            match.NotifyClientsAboutPlayedCard(LibraryCards.GetCard(cardGuid), "");
            match.SendOutMatchDetails();
        }
    }
}

#endif