using System;
using Core.Map;
using Mirror;

namespace Core.Contracts
{
    public struct PveMatchDetailsDto : NetworkMessage
    {
        public Guid PlayerId { get; set; }
        
        public string[] CardsInHandIds { get; set; }
        
        public Fatigue Fatigue { get; set; }
        
        public MatchPlayerDto[] Players { get; set; }
        
        public LevelInfo LevelInfo { get; set; }
        
        public bool IsYourTurn { get; set; }
    }
}