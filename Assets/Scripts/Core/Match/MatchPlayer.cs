using Core.Cards;
using Core.Castle;
using JetBrains.Annotations;
using Mirror;

namespace Core.Match
{
    public class MatchPlayer
    {
        public string PlayFabId { get; set; }
        
        public string Name { get; set; }
        
        public CastleEntity Castle { get; set; }
        
        public int Division { get; set; }

        public bool IsReady { get; set; } = false;
        
        [CanBeNull] public PlayerCards PlayerCards { get; set; }
        
        [CanBeNull] public NetworkConnectionToClient Connection { get; set; }
    }
}