using System;

namespace Core.Map
{
    [Serializable]
    public class BiomeProgress
    {
        public int BiomeId { get; set; }

        public int Progress { get; set; } = 0;

        public int MaxProgress { get; set; } = 5; //Never changed

        public bool IsCompleted() => Progress >= MaxProgress;
    }
}