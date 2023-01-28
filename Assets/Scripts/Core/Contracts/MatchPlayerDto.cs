using System;
using Core.Castle;
using Mirror;

namespace Core.Contracts
{
    public struct MatchPlayerDto : NetworkMessage
    {
        public string PlayerId { get; set; }
        
        public string Name { get; set; }
        
        public CastleEntity Castle { get; set; }
    }
}