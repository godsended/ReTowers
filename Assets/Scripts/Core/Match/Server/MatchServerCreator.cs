#if !UNITY_ANDROID

namespace Core.Match.Server
{
    public partial class MatchServer
    {
        public class MatchServerCreator
        {
            public MatchServer CrateMatchServer()
            {
                return new MatchServer(false);
            }

            public MatchServer CreateBotMatchServer(bool isPve)
            {
                return new MatchServer(isPve);
            }
        }
    }
}

#endif