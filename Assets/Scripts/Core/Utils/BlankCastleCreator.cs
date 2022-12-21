using Core.Castle;

namespace Core.Utils
{
    public class BlankCastleCreator : ICastleCreator
    {
        public CastleEntity CreateCastle()
        {
            return new CastleEntity();
        }
    }
}