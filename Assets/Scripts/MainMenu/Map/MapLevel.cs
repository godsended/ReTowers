using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.Map
{
    public class MapLevel : MonoBehaviour
    {
        private bool isAvailable;

        [SerializeField] private int progress;

        [SerializeField] private int biomeId;

        [SerializeField] private int levelId;

        [SerializeField] private Animator animator;

        [SerializeField] private Button button;

        [SerializeField] private MapController mapController;

        public int BiomeId => biomeId;

        public int Progress => progress;

        public int LevelId => levelId;

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
            mapController.SetLevelId(levelId);
        }
    }
}