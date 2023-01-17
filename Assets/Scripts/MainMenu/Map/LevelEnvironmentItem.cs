using System;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.Map
{
    public class LevelEnvironmentItem : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        [SerializeField] private MapLevel relatedLevel;

        [SerializeField] private Image image;

        private void Awake()
        {
            if(relatedLevel != null)
                relatedLevel.OnAvailabilityChanged += HandleAvailabilityChanges;

            if (image == null)
            {
                TryGetComponent(out image);
            }

            if (animator == null)
                TryGetComponent(out animator);
        }

        private void HandleAvailabilityChanges(object sender, EventArgs e)
        {
            animator.enabled = relatedLevel.IsAvailable;
            image.color = relatedLevel.IsAvailable ? Color.white : Color.grey;
        }
    }
}