using System.Collections.Generic;
using Core.Economics;
using Mirror;

namespace Core.Contracts
{
    public struct WalletDto : NetworkMessage
    {
        public string PlayFabId { get; set; }
        
        public Dictionary<string, Currency> Balance { get; set; }
        
        public WalletRequestType RequestType { get; set; }
    }

    public enum WalletRequestType
    {
        Balance = 0,
        Reward = 1
    }
}