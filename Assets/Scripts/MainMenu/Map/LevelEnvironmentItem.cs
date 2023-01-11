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
        }

        private void HandleAvailabilityChanges(object sender, EventArgs e)
        {
            animator.enabled = relatedLevel.IsAvailable;
            image.color = relatedLevel.IsAvailable ? Color.white : Color.grey;
        }
    }
}