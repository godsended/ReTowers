using System;
using System.Linq;
using UnityEngine;
using Core.Map;
using Core.Map.Client;
using Settings;

namespace MainMenu.Map
{
    public class MapUIController : MonoBehaviour
    {
        [SerializeField] private MapLevel[] levels;

        [SerializeField] private SettingsManager settingsManager;

        private ClientMapController clientMapController;

        public MapProgress Progress { get; set; } = null;

        private void Start()
        {
            foreach (var level in levels)
            {
                level.settingsManager = settingsManager;
            }
            
            clientMapController = new ClientMapController();
            clientMapController.OnProgressUpdated += OnServerProgressReceived;
            clientMapController.OnProgressUpdateFailed += OnServerProgressFailedToReceive;
            
            Init();
            
            clientMapController.RequestMapProgress();
        }

        public void SetNullPointIndex()
        {
            ScensVar.LevelId = -1;
        }

        public void SetLevelId(int levelId)
        {
            ScensVar.LevelId = levelId;
        }

        public void Init(MapProgress mapProgress = null)
        {
            Progress = mapProgress ?? new MapProgressBuilder().AddBiomes(3).Build();;
            foreach (var biome in Progress.Biomes)
            {
                foreach (var level in levels)
                {
                    if(level.BiomeId != biome.BiomeId)
                        continue;

                    level.IsAvailable = biome.Progress >= level.Progress;
                }
            }
        }

        private void OnServerProgressReceived(object sender, EventArgs e)
        {
            Init(clientMapController.Progress);
        }

        private void OnServerProgressFailedToReceive(object sender, EventArgs e)
        {
        }

        private void OnEnable()
        {
            if (clientMapController == null)
                return;
            
            clientMapController.RequestMapProgress();
        }
    }
}