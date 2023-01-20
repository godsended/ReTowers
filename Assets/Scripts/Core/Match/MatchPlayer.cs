using Core.Cards;
using Core.Castle;
using Mirror;

namespace Core.Match
{
    public class MatchPlayer
    {
        public string PlayFabId { get; set; }
        
        public string Name { get; set; }
        
        public CastleEntity Castle { get; set; }
        
        public PlayerCards PlayerCards { get; set; }
        
        public NetworkConnectionToClient Connection { get; set; }
    }
}