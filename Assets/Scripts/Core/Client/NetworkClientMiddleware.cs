using System;
using Mirror;
using UnityEngine;

namespace Core.Client
{
    public class NetworkClientMiddleware : MonoBehaviour
    {
        private static NetworkClientMiddleware instance;

        [SerializeField] private GameObject connectionPopupObject;
        
        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(this);
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShowNoConnectionPopup()
        {
            connectionPopupObject.SetActive(true);
        }

        public void HideConnectionPopupObject()
        {
            connectionPopupObject.SetActive(false);
        }

        public static void Send<T>(T message, int channelId = Channels.Reliable) where T : struct, NetworkMessage
        {
            if (NetworkClient.connection == null || !NetworkClient.isConnected)
            {
                if(instance != null)
                    instance.ShowNoConnectionPopup();
            }
            
            NetworkClient.Send(message, channelId);
        }
    }
}