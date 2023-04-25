using System;
using System.Linq;
using Core.Client;
using Core.Contracts;
using MainMenu.Registration;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Economics.Client
{
    public class ClientWalletController : MonoBehaviour
    {
        public static ClientWalletController Instance { get; private set; }
        
        public Wallet Wallet { get; } = new Wallet();

        public event Currency.CurrencyHandler OnRewardRecieved;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            NetworkClient.RegisterHandler<WalletDto>(HandleWalletRequest);
            AuthClientController.authSuccessEvent.AddListener(RequestWalletInfo);
        }

        private void HandleWalletRequest(WalletDto dto)
        {
            Debug.Log("Request accepted\n" + JsonConvert.SerializeObject(dto));
            switch (dto.RequestType)
            {
                case WalletRequestType.Balance:
                    HandleBalanceRequest(dto);
                    break;
                case WalletRequestType.Reward:
                    HandleRewardRequest(dto);
                    break;
            }
        }

        private void HandleBalanceRequest(WalletDto dto)
        {
            foreach (var currency in dto.Balance.Values)
            {
                Wallet[currency.Name].Amount = currency.Amount;
            }

            foreach (var currency in Wallet.Balance.Values)
            {
                if (!dto.Balance.ContainsKey(currency.Name))
                    currency.Amount = 0;
            }
        }

        private void HandleRewardRequest(WalletDto dto)
        {
            if (dto.Balance.Values.Count == 0)
                return;
            
            OnRewardRecieved?.Invoke(dto.Balance.First().Value);
        }
        

        public void RequestWalletInfo()
        {
            Debug.Log("Wallet info request sended!");
            NetworkClient.Send(new WalletDto()
            {
                PlayFabId = PlayfabManager.playerId
            });
        }
    }
}