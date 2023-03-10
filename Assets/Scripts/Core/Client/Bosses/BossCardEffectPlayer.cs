using System;
using UnityEngine;

namespace Core.Client.Bosses
{
    [RequireComponent(typeof(BossView))]
    public class BossCardEffectPlayer : MonoBehaviour
    {
        private BossView bossView;

        public GameObject effect;

        private void Awake()
        {
            bossView = GetComponent<BossView>();
            bossView.OnPlayCastAnimation += BossViewOnOnPlayCastAnimation;
        }

        private void BossViewOnOnPlayCastAnimation(object sender, EventArgs e)
        {
            Instantiate(effect, Vector3.zero, Quaternion.identity);
        }
    }
}