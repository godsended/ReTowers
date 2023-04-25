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
    public class PveMatchRewardModificator
    {
        private MatchServer match;

        private Currency reward;
        
        public PveMatchRewardModificator(MatchServer match, Currency reward)
        {
            this.reward = reward;
            this.match = match;
            this.match.OnPlayerLeaved += OnPlayerLeaved;
        }

        private void OnPlayerLeaved(MatchPlayer player, bool isWin)
        {
            if (!isWin || player is IMatchBot) 
                return;
            var inc = new Dictionary<string, double>();
            inc.Add(reward.Name, reward.Amount);
            ServerWalletController.Instance.IncreaseBalanceBy(player.PlayFabId, inc);
            
            WalletDto dto = new()
            {
                Balance = new Dictionary<string, Currency> {{reward.Name, reward}},
                RequestType = WalletRequestType.Reward
            };
            MainServer.instance.AuthPlayers
                .FirstOrDefault(p => p.PlayFabId == player.PlayFabId)?.Connection.Send(dto);
        }
    }
}

#endif