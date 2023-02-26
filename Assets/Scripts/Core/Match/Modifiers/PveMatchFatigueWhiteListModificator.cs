using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Map;
using Core.Match.Server;

namespace Core.Match.Modifiers
{
    public class PveMatchFatigueWhiteListModificator : IMatchModificator
    {
        private IEnumerable<MatchPlayer> whiteList;
        
        public PveMatchFatigueWhiteListModificator(MatchServer match, IEnumerable<MatchPlayer> whiteList)
        {
            match.FatigueFilter += WillThePlayerBeDamaged;
            this.whiteList = whiteList;
        }

        private bool WillThePlayerBeDamaged(MatchPlayer player)
        {
            return !whiteList.Contains(player);
        }
    }
}