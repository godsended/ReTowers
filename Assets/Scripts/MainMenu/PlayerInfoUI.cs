using System;
using System.Collections;
using Core.Client;
using Core.Contracts;
using MainMenu.Registration;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace MainMenu
{
    public class PlayerInfoUI : MonoBehaviour
    {
        public static PlayerInfoUI instance;
        
        [Header("UI")] 
        [SerializeField] 
        public GameObject username;
        [SerializeField] 
        private TextMeshProUGUI playerWinCountText;
        [SerializeField] 
        private TextMeshProUGUI enemyWinCountText;
        [SerializeField]
        private TextMeshProUGUI energyCountText;

        public void UpdatePlayerInfo()
        {
            if (playerWinCountText is null)
                return;
            
            playerWinCountText.SetText(MainClient.GetWinCount().ToString());

            if (energyCountText is null)
                return;

            energyCountText.SetText(MainClient.GetEnergyCount().ToString() + "/20");

            if (username == null)
                return;
            
            var text = username.GetComponent<TextMeshProUGUI>();
            
            text.SetText(MainClient.GetUsername());
        }

        private void Start()
        {
            instance = this;
            StartCoroutine("GetStatistics");
        }

        private void Update()
        {
            var time = 0.0f;
            var interpolationPeriod = 0.5f;

            time += Time.deltaTime;

            if (time >= interpolationPeriod)
            {
                time = 0.0f;

                UpdatePlayerInfo();
            }
        }

        IEnumerator GetStatistics()
        {
            if (PlayfabManager.playerId == null) 
                yield break;
            
            PlayfabManager.instance.GetStatistics();
            UpdatePlayerInfo();
            yield return new WaitForSeconds(5f);

        }
    }
}
