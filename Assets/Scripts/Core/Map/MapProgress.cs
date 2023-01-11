using System;
using System.Linq;

namespace Core.Map
{
    [Serializable]
    public class MapProgress
    {
        public BiomeProgress[] Biomes { get; set; }
        
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        public bool IsCompleted() => Biomes.All(b => b.IsCompleted());
    }
}