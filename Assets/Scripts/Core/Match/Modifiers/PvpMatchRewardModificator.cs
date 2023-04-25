#if !UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts;
using Core.Economics;
using Core.Economics.Server;
using Core.Match.Server;
using Core.Server;

namespace Core.Match.Modifiers
{
    public class PvpMatchRewardModificator : IMatchModificator
    {
        private MatchServer match;
        
        public PvpMatchRewardModificator(MatchServer match)
        {
            this.match = match;
            this.match.OnPlayerLeaved += OnPlayerLeaved;
        }

        private void OnPlayerLeaved(MatchPlayer player, bool isWin)
        {
            if (!isWin || player is IMatchBot) 
                return;
            
            var inc = new Dictionary<string, double> {{"Pvp", Math.Pow(1.5, match.MatchDetails.Division - 1)}};
            ServerWalletController.Instance.IncreaseBalanceBy(player.PlayFabId, inc);

            var first = inc.First();
            WalletDto dto = new()
            {
                Balance = new Dictionary<string, Currency> {{first.Key, new Currency(first.Key, first.Value)}},
                RequestType = WalletRequestType.Reward
            };
            MainServer.instance.AuthPlayers
                .FirstOrDefault(p => p.PlayFabId == player.PlayFabId)?.Connection.Send(dto);
        }
    }
}

#endif