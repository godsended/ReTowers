using System;
using Core.Map;
using Settings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MainMenu.Map
{
    public class MapLevel : MonoBehaviour
    {
        private bool isAvailable;

        [HideInInspector] public SettingsManager settingsManager;

        [SerializeField] private LevelInfo levelInfo;

        [SerializeField] private Animator animator;

        [SerializeField] private Button button;

        [FormerlySerializedAs("mapController")] [SerializeField] private MapUIController mapUIController;

        public int BiomeId => levelInfo.BiomeId;

        public int Progress => levelInfo.Progress;

        public int LevelId => levelInfo.LevelId;

        public event EventHandler OnAvailabilityChanged;

        public bool IsAvailable
        {
            get => isAvailable;
            set
            {
                isAvailable = value;
                IsAnimated = value;
                button.interactable = value;
                OnAvailabilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsAnimated
        {
            get => animator.enabled;
            set => animator.enabled = value;
        }

        public void ChangePointIndex()
        {
            mapUIController.SetLevelId(levelInfo.LevelId);
        }

        public void StartBotGame()
        {
            settingsManager.SearchBotMatch(LevelId);
        }
    }
}