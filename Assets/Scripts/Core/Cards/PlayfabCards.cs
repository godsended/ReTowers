using UnityEngine;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System;

namespace Core.Cards {
    public class PlayfabCards : MonoBehaviour
    {
        public static void SaveCards(List<CardJson> cards)
        {
            var request = new UpdateUserDataRequest {
                Data = new Dictionary<string, string>{
                { "PlayerCards", JsonConvert.SerializeObject(cards) }
            }
            };
            PlayFabClientAPI.UpdateUserData(request, null, null);
        }

        public static void GetPlayerCards(Action<GetUserDataResult> OnPlayerDeckDataRecieved)
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnPlayerDeckDataRecieved, null);
        }
    }
}
