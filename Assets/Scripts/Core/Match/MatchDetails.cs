using System.Collections.Generic;
using Core.Map;
using Core.Match;
using Core.Utils;
using JetBrains.Annotations;
using Mirror;

namespace Core.Match
{
    public class MatchDetails : NetworkMessage
    {
        public List<MatchPlayer> Players { get; private set; }

        [CanBeNull] public MatchPlayer CurrentPlayer => TurnPlayer >= 0 ? Players[TurnPlayer] : null;

        [CanBeNull] public MatchPlayer NextPlayer => NextTurnPlayer >= 0 ? Players[NextTurnPlayer] : null;
        
        public Fatigue Fatigue { get; set; }
        
        public int Division { get; set; }
        
        public double TurnTime { get; set; }
        
        public double PrepareTime { get; set; }
        
        public int Turn { get; set; }
        
        public int FatigueTurn { get; set; }

        public int TurnPlayer => Players.Count > 0 ? Turn % Players.Count : -1;

        public int NextTurnPlayer => TurnPlayer >= 0 ? (TurnPlayer + 1) % Players.Count : -1;
        
        [CanBeNull] public LevelInfo LevelInfo { get; set; }
        
        [CanBeNull] public MapProgress MapProgress { get; set; }

        public MatchDetails()
        {
            Division = 1;
            Players = new List<MatchPlayer>();
        }
    }
}