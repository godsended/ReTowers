#if !UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts;
using Core.Server;
using Mirror;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using UnityEngine;

namespace Core.Economics.Server
{
    public class ServerWalletController : MonoBehaviour
    {
        public static ServerWalletController Instance { get; private set; }

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
            NetworkServer.RegisterHandler<WalletDto>(HandleWalletRequest, false);
        }

        private void HandleWalletRequest(NetworkConnectionToClient connection, WalletDto walletDto)
        {
            Debug.Log($"Wallet info requested by {walletDto.PlayFabId}");
            var request = new GetUserDataRequest
            {
                PlayFabId = walletDto.PlayFabId
            };

            PlayFabServerAPI.GetUserData(request,
                result =>
                {
                    walletDto.Balance = GetBalanceFromRequest(result);
                    walletDto.RequestType = WalletRequestType.Balance;
                    connection.Send(walletDto);
                },
                Debug.LogError);
        }

        public void IncreaseBalanceBy(string playFabId, Dictionary<string, double> changes)
        {
            var request = new GetUserDataRequest
            {
                PlayFabId = playFabId
            };

            PlayFabServerAPI.GetUserData(request,
                result =>
                {
                    var walletDto = new WalletDto
                    {
                        Balance = GetBalanceFromRequest(result),
                        RequestType = WalletRequestType.Balance
                    };

                    foreach (var change in changes)
                    {
                        if(!walletDto.Balance.ContainsKey(change.Key))
                            walletDto.Balance.Add(change.Key, new Currency(change.Key, 0));
                        walletDto.Balance[change.Key].Amount += change.Value;
                    }
                    
                    SendBalanceToPlayFab(playFabId, walletDto.Balance);
                    
                    MainServer.instance.AuthPlayers.FirstOrDefault(p => p.PlayFabId == playFabId)?.Connection.Send(walletDto);
                },
                Debug.LogError);
        }

        private Dictionary<string, Currency> GetBalanceFromRequest(GetUserDataResult result)
        {
            return !result.Data.ContainsKey(PlayFabDataKeys.Balance) 
                ? new Dictionary<string, Currency>() 
                : JsonConvert.DeserializeObject<Dictionary<string, Currency>>(result.Data[PlayFabDataKeys.Balance].Value);
        }

        private void SendBalanceToPlayFab(string playFabId, Dictionary<string, Currency> balance)
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {PlayFabDataKeys.Balance, JsonConvert.SerializeObject(balance)}
                },
                PlayFabId = playFabId
            };
            
            PlayFabServerAPI.UpdateUserData(request, 
                result => Debug.Log("Balance updated"), 
                error => Debug.LogError("Balance update error:\n" + error
                ));
        }
    }
}
#endif