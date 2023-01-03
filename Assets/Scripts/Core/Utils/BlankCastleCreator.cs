using Core.Castle;

namespace Core.Utils
{
    public class BlankCastleCreator : ICastleCreator
    {
        public BlankCastleCreator() { }
        public CastleEntity CreateCastle()
        {
            return new CastleEntity();
        }
    }
}