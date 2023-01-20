using Core.Match.Server;

namespace Core.Match
{
    public class MatchBot : MatchPlayer, IMatchBot
    {
        private MatchServer server;
        public MatchBot(MatchServer server)
        {
            PlayFabId = "";
            
        }
    }
}