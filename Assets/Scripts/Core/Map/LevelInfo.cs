using UnityEngine;

namespace Core.Map
{
    [CreateAssetMenu(fileName = "New level info", menuName = "Levels/New level info")]
    public class LevelInfo : ScriptableObject
    {
        public int LevelId = -1;

        public int BiomeId = -1;

        public int Progress = -1;
    }
}