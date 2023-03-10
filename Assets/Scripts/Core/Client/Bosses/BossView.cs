using System;
using UnityEngine;

namespace Core.Client.Bosses
{
    public class BossView : MonoBehaviour, IBossView
    {
        private static readonly int Cast = Animator.StringToHash("Cast");
        
        [SerializeField] private Animator animator;

        public event EventHandler OnPlayCastAnimation;

        public event EventHandler OnPlayHitAnimation;
        
        public event EventHandler OnPlayCardAnimation;

        public void PlayCastAnimation()
        {
            animator.SetTrigger(Cast);
            OnPlayCastAnimation?.Invoke(this, EventArgs.Empty);
        }

        public void PlayHitAnimation()
        {
            OnPlayHitAnimation?.Invoke(this, EventArgs.Empty);
        }

        public void PlayIdleAnimation()
        {
            
        }

        public void PlayCardAnimation()
        {
            OnPlayCardAnimation?.Invoke(this, EventArgs.Empty);
        }
    }
}