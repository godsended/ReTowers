#if !UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Castle;
using Core.Match.Server;
using UnityEngine;

namespace Core.Match.Modifiers
{
    public class PveMatchCastlesModificator : IMatchModificator
    {
        private MatchServer match;

        private MatchCastleAddition myCastleAddition, botCastleAddition;

        public PveMatchCastlesModificator(MatchServer match, MatchCastleAddition myCastleAddition,
            MatchCastleAddition botCastleAddition)
        {
            this.match = match;
            match.OnMatchStarted += OnMatchStarted;
            this.myCastleAddition = myCastleAddition;
            this.botCastleAddition = botCastleAddition;
        }

        private void OnMatchStarted(object sender, EventArgs e)
        {
            if (sender != match)
                return;
            
            var bot = match.MatchDetails.Players.FirstOrDefault(p => p is IMatchBot);
            var player = match.MatchDetails.Players.FirstOrDefault(p => p is not IMatchBot);
            if (player != null)
            {
                myCastleAddition.ApplyAddition(player.Castle);
            }
            else
            {
                Debug.Log("BotMatchCastlesModificator: player not found");
            }
            
            if (bot != null)
            {
                botCastleAddition.ApplyAddition(bot.Castle);
            }
            else
            {
                Debug.Log("BotMatchCastlesModificator: bot not found");
            }
        }
    }

    public struct MatchCastleAddition
    {
        public int TowerAddition;

        public int WallAddition;

        public IEnumerable<BattleResource> ResourcesAddition;
        
        public static readonly MatchCastleAddition Empty = new()
        {
            TowerAddition = 0,
            WallAddition = 0,
            ResourcesAddition = Array.Empty<BattleResource>()
        };

        public void ApplyAddition(CastleEntity castle)
        {
            castle.Wall.Health += WallAddition;
            castle.Tower.Health += TowerAddition;
            foreach (var addition in ResourcesAddition)
            {
                var res = castle.Resources.FirstOrDefault(r => r.Name == addition.Name);
                if (res == null) continue;
                
                res.AddResource(addition.Value);
                res.AddIncome(addition.Income);
            }
        }
    }
}

#endif