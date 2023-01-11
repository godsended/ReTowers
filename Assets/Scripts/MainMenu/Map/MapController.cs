using System;
using System.Linq;
using UnityEngine;
using Core.Map;

namespace MainMenu.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private MapLevel[] levels;

        public MapProgress Progress { get; set; } = null;

        private void Start()
        {
            Init();
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

        //private void Start()
        //{
            // if (PlayerPrefs.HasKey("Time"))
            // {
            //     DateTime oldTime = DateTime.Parse(PlayerPrefs.GetString("Time"));
            //     Debug.Log(oldTime);
            //     if (DateTime.Now.Day != oldTime.Day)
            //     {
            //         PlayerPrefs.DeleteKey("Points");
            //     }
            // }
            //
            // PlayerPrefs.SetString("Time", DateTime.Now.ToString());
            //
            // if (PlayerPrefs.HasKey("Points"))
            // {
            //     _activetedPoints = PlayerPrefs.GetString("Points");
            // }
            // else
            // {
            //     _activetedPoints = "";
            //
            //     for (int i = 0; i < points.Length; i++)
            //     {
            //         _activetedPoints += "0";
            //     }
            //
            //     PlayerPrefs.SetString("Points", _activetedPoints);
            // }
            //
            // PlayerPrefs.Save();
            //
            // for (int i = 0; i < points.Length; i++)
            // {
            //     Debug.Log(_activetedPoints[i]);
            //     if (_activetedPoints[i] == '1')
            //     {
            //         points[i].SetPointActiv(false);
            //     }
            // }
        //}
    }
}