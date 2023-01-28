using System;
using Core.Map;
using Mirror;

namespace Core.Contracts
{
    public struct MatchDetailsDto : NetworkMessage
    {
        public string PlayerId { get; set; }
        
        public string[] CardsInHandIds { get; set; }
        
        public Fatigue Fatigue { get; set; }
        
        public MatchPlayerDto[] Players { get; set; }
        
        public LevelInfo LevelInfo { get; set; }
        
        public bool IsYourTurn { get; set; }
    }
}