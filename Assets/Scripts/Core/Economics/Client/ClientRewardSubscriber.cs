using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Economics.Client
{
    public class ClientRewardSubscriber : MonoBehaviour
    {
        public Image rewardImage;
        public TMP_Text rewardTextField;
        
        [Header("Reward sprite by name")]
        public List<string> rewardsNames = new ();
        public List<Sprite> rewardsSprites = new ();

        private void Start()
        {
            rewardImage.gameObject.SetActive(false);
            rewardTextField.gameObject.SetActive(false);
            ClientWalletController.Instance.OnRewardRecieved += SetRewardInfo;
        }

        private void SetRewardInfo(Currency reward)
        {
            rewardImage.gameObject.SetActive(true);
            rewardTextField.gameObject.SetActive(true);
            
            int i = rewardsNames.IndexOf(reward.Name);
            if (i < 0 || i > rewardsSprites.Count)
                return;

            rewardImage.sprite = rewardsSprites[i];
            rewardTextField.text = $"{reward.Amount}x";
        }

        public void OnDestroy()
        {
            ClientWalletController.Instance.OnRewardRecieved -= SetRewardInfo;
        }
    }
}
