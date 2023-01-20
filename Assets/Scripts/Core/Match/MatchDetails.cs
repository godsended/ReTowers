using System.Collections.Generic;
using Core.Match;
using Core.Utils;
using Mirror;

namespace Core.Match
{
    public class MatchDetails : NetworkMessage
    {
        public List<MatchPlayer> Players { get; private set; }

        public MatchPlayer CurrentPlayer => Players[TurnPlayer];

        public MatchPlayer NextPlayer => Players[NextTurnPlayer];
        
        public Fatigue Fatigue { get; set; }
        
        public int Division { get; set; }
        
        public double TurnTime { get; set; }
        
        public double PrepareTime { get; set; }
        
        public int Turn { get; set; }
        
        public int FatigueTurn { get; set; }

        public int TurnPlayer => Turn % Players.Count;

        public int NextTurnPlayer => (TurnPlayer + 1) % Players.Count;

        public MatchDetails()
        {
            Division = 1;
            Players = new List<MatchPlayer>();
        }

        public virtual void Init()
        {
            Fatigue = new Fatigue(Division);
            foreach (var player in Players)
            {
                player.Castle = new DivisionCastleCreator(Division).CreateCastle();
            }
        }
    }
}