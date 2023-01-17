using UnityEngine;

namespace Core.Map
{
    [CreateAssetMenu(fileName = "New level info", menuName = "Levels/New level info")]
    public class LevelInfo : ScriptableObject
    {
        public int LevelId;

        public int BiomeId;

        public int Progress;
    }
}