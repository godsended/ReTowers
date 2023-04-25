#if !UNITY_ANDROID

using System;
using System.Linq;
using Core.Match.Server;
using JetBrains.Annotations;

namespace Core.Match.Modifiers
{
    public class PveMatchUnlimitedFatigueModificator : IMatchModificator
    {
        private MatchServer match;

        private Fatigue additionalFatigue;

        private bool skipNextDamage = false;

        [CanBeNull] private MatchPlayer targetPlayer;

        public PveMatchUnlimitedFatigueModificator(MatchServer match)
        {
            this.match = match;
            this.match.OnMatchStarted += OnMatchStarted;
            this.match.OnFatigueDamaged += OnFatigueDamagedPlayer;
            this.match.BeforeFatigueDamaged += BeforeFatigueDamagedPlayer;
            targetPlayer = match.MatchDetails.Players.FirstOrDefault(p => p is not IMatchBot);
        }

        private void OnMatchStarted(object sender, EventArgs e)
        {
            if (sender != match)
                return;
            
            additionalFatigue = new Fatigue(match.MatchDetails.Division)
            {
                MaxDamage = int.MaxValue
            };
        }

        private void OnFatigueDamagedPlayer(object sender, EventArgs e)
        {
            if (targetPlayer == null || sender != match 
                                     || match.MatchDetails.Fatigue.Damage != match.MatchDetails.Fatigue.MaxDamage)
                return;

            if (!skipNextDamage)
            {
                match.DamagePlayer(targetPlayer, additionalFatigue.Damage, false);
                skipNextDamage = false;
            }

            additionalFatigue++;
        }

        private void BeforeFatigueDamagedPlayer(object sender, EventArgs e)
        {
            if (sender != match || targetPlayer == null)
                return;

            if (targetPlayer.Castle.Wall.Health <= match.MatchDetails.Fatigue.Damage)
                skipNextDamage = true;
        }
    }
}

#endif