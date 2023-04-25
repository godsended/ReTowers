#if !UNITY_ANDROID

using Core.Contracts;
using Mirror;
using System;
using UnityEngine;

namespace Core.Server
{
    public class AuthServerController : MonoBehaviour
    {
        public static AuthServerController instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            NetworkServer.RegisterHandler<AuthDto>(HandleRequestAuth, false);
        }

        public void HandleRequestAuth(NetworkConnectionToClient connection, AuthDto authDto)
        {
            Guid testGuid = Guid.NewGuid();

            if (authDto.IsGuest)
            {
                MainServer.AddAuthPlayer(new PlayerData
                {
                    Id = testGuid,
                    Name = "Guest", 
                    Connection = connection,
                    IsGuest = true
                });
            }
            else
            {
                MainServer.AddAuthPlayer(new PlayerData
                {
                    Id = testGuid,
                    Name = authDto.Name, 
                    Connection = connection,
                    IsGuest = false,
                    PlayFabId = authDto.Login,
                    LastLoginTime = DateTime.UtcNow
                });
            }
            

            connection.Send(new AccountDto
            {
                Id = testGuid,
                Login = authDto.Login,
                IsGuest = authDto.IsGuest
            });
        }
    }
}

#endif