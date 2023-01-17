using System.Collections.Generic;
using UnityEngine;

namespace Core.Map
{
    [CreateAssetMenu(fileName = "New levels configuration", menuName = "Levels/New configuration")]
    public class LevelsConfiguration : ScriptableObject
    {
        public List<LevelInfo> Levels;
    }
}