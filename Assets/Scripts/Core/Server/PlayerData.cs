using Core.Cards;
using Core.Castle;
using Mirror;
using System;
using Core.Map;

namespace Core.Server
{
    /// <summary>
    /// Entity player
    /// </summary>
    public class PlayerData
    {
        public PlayerData()
        {
            PlayerStatistics = new PlayerStatistics();
        }

        public Guid Id { get; set; }
        public string PlayFabId { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool IsGuest { get; set; }
        public string Name { get; set; }       
        public NetworkConnectionToClient Connection { get; set; }       
        public Match CurrentMatch { get; set; }       
        public PlayerCards Cards { get; set; }        
        public CastleEntity Castle { get; set; }
        public PlayerStatistics PlayerStatistics { get; set; }
        public int Division { get; set; }
        
        public MapProgress MapProgress { get; set; }
    }
}