using Core.Map;

namespace Core.Match
{
    public class PveMatchDetails : MatchDetails
    {
        public LevelInfo LevelInfo { get; set; }
        
        public MapProgress MapProgress { get; set; }
    }
}