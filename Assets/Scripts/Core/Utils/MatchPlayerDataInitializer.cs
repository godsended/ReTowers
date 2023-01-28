using Core.Match;
using Core.Server;

namespace Core.Utils
{
    public class MatchPlayerDataInitializer<T> where T : ICastleCreator
    {
        private T castleCreator;
        
        public MatchPlayerDataInitializer(T castleCreator)
        {
            this.castleCreator = castleCreator;
        }

        public void Initialize(MatchPlayer playerData)
        {
            playerData.Castle = castleCreator.CreateCastle();
        }
    }
}