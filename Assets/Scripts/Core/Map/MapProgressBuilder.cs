using System;
using System.Collections.Generic;

namespace Core.Map
{
    public class MapProgressBuilder
    {
        private MapProgress mapProgress;

        private List<BiomeProgress> biomes;

        public MapProgressBuilder()
        {
            Reset();
        }

        public MapProgressBuilder AddBiomes(int count = 1)
        {
            if (count < 1)
                return this;

            for (int i = 0; i < count; i++)
            {
                biomes.Add(new BiomeProgress() {BiomeId = biomes.Count});
            }

            return this;
        }

        public MapProgressBuilder SetLastUpdated(DateTime value)
        {
            mapProgress.LastUpdated = value;
            return this;
        }

        public MapProgress Build()
        {
            mapProgress.Biomes = biomes.ToArray();
            return mapProgress;
        }

        public MapProgressBuilder Reset()
        {
            mapProgress = new MapProgress();
            biomes = new List<BiomeProgress>();

            return this;
        }
    }
}