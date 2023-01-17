using Core.Map;
using Mirror;

namespace Core.Contracts
{
    public struct MapProgressDto : NetworkMessage
    {
        public string PlayFabId { get; set; }
        
        public MapProgress Progress { get; set; }
        
        public bool IsError { get; set; }
    }
}