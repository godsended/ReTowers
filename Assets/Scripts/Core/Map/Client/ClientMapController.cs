using System;
using Core.Client;
using Core.Contracts;
using MainMenu.Registration;
using Mirror;
using UnityEngine;

namespace Core.Map.Client
{
    public class ClientMapController
    {
        public event EventHandler OnProgressUpdated;

        public event EventHandler OnProgressUpdateFailed;

        public MapProgress Progress { get; set; }

        public ClientMapController()
        {
            NetworkClient.RegisterHandler<MapProgressDto>(OnMapRequestResponse, false);
        }

        public void RequestMapProgress()
        {
            MapProgressDto dto = new MapProgressDto();
            if (string.IsNullOrEmpty(PlayfabManager.playerId))
            {
                Debug.Log("PlayerId is null");
                return;
            }
            dto.PlayFabId = PlayfabManager.playerId;
            Debug.Log(dto.PlayFabId);
            NetworkClientMiddleware.Send(dto);
        }

        private void OnMapRequestResponse(MapProgressDto dto)
        {
            if (dto.IsError)
            {
                OnProgressUpdateFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            Progress = dto.Progress;
            OnProgressUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}