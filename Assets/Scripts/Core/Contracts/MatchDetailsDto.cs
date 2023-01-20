using System;
using Mirror;

namespace Core.Contracts
{
    public struct MatchDetailsDto : NetworkMessage
    {
        public Guid PlayerId { get; set; }
        
        public string[] CardsInHandIds { get; set; }
        
        public Fatigue Fatigue { get; set; }
        
        public MatchPlayerDto[] Players { get; set; }
        
        public bool IsYourTurn { get; set; }
    }
}