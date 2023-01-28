using Mirror;

namespace Core.Contracts
{
    public struct LoadBattleSceneDto : NetworkMessage
    {
        public string RequestId { get; set; }
        
        public string MatchId { get; set; }
    }
}