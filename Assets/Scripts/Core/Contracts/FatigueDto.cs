using System;
using Mirror;

namespace Core.Contracts
{
    public struct FatigueDto : NetworkMessage
    {
        public Guid PlayerId { get; set; }
        
        public int Damage { get; set; }
    }
}