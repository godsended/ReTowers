#if !UNITY_ANDROID
using System;
using System.Collections.Generic;
using Core.Contracts;
using Newtonsoft.Json;
using Mirror;
using PlayFab;
using PlayFab.ServerModels;
using UnityEngine;

namespace Core.Map.Server
{
    public class ServerMapController : MonoBehaviour
    {
        public static ServerMapController Instance { get; private set; }

        [SerializeField] private LevelsConfiguration levelsConfiguration;
        
        public LevelsConfiguration LevelsConfiguration => levelsConfiguration;

        private ServerMapController()
        {
            if(Instance != null)
                Destroy(this);
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }

            Instance = this;
        }

        private void Start()
        {
            NetworkServer.RegisterHandler<MapProgressDto>(HandleMapRequest, false);
        }

        private void HandleMapRequest(NetworkConnectionToClient connection, MapProgressDto dto)
        {
            Debug.Log("Accepted");
            if (string.IsNullOrEmpty(dto.PlayFabId))
            {
                Debug.Log("Play fab id is empty");
                return;
            }

            Debug.Log(dto.PlayFabId);

            var request = new GetUserDataRequest
            {
                PlayFabId = dto.PlayFabId
            };

            PlayFabServerAPI.GetUserData(request,
                result =>
                {
                    MapProgress progress;
                    
                    if (!result.Data.ContainsKey(PlayFabDataKeys.MapProgress))
                    {
                        progress =
                            new MapProgressBuilder().AddBiomes(3).SetLastUpdated(DateTime.UtcNow).Build();
                        connection.Send(new MapProgressDto()
                        {
                            Progress = progress,
                            IsError = false,
                            PlayFabId = dto.PlayFabId
                        });

                        SendProgressToPlayFab(progress, dto.PlayFabId);
                        return;
                    }

                    try
                    {
                        progress = UpdateMapProgress(
                            JsonConvert.DeserializeObject<MapProgress>(result.Data[PlayFabDataKeys.MapProgress].Value));
                        
                        SendProgressToPlayFab(progress, dto.PlayFabId);
                        
                        MapProgressDto answer = new MapProgressDto();
                        answer.PlayFabId = result.PlayFabId;
                        answer.Progress =
                            progress;
                        answer.IsError = false;
                        connection.Send(answer);
                    }
                    catch (JsonSerializationException e)
                    {
                        connection.Send(new MapProgressDto {IsError = true});
                        Debug.LogError(e);
                    }
                    catch (Exception e)
                    {
                        connection.Send(new MapProgressDto {IsError = true});
                        Debug.LogError("Unhandled exception:\n" + e);
                    }
                },
                error =>
                {
                    Debug.LogError(error);
                    connection.Send(new MapProgressDto {IsError = true});
                });
        }

        public void LoadMapProgress(string playFabId, Action<MapProgress> action)
        {
            var request = new GetUserDataRequest
            {
                PlayFabId = playFabId
            };
            
            PlayFabServerAPI.GetUserData(request, result =>
            {
                MapProgress progress;
                if (!result.Data.ContainsKey(PlayFabDataKeys.MapProgress))
                {
                    progress =
                        new MapProgressBuilder().AddBiomes(3).SetLastUpdated(DateTime.UtcNow).Build();
                    
                    SendProgressToPlayFab(progress, playFabId);
                    action(progress);
                    return;
                }
                
                try
                {
                    progress = UpdateMapProgress(
                        JsonConvert.DeserializeObject<MapProgress>(result.Data[PlayFabDataKeys.MapProgress].Value));
                        
                    SendProgressToPlayFab(progress, playFabId);
                    action(progress);
                }
                catch (JsonSerializationException e)
                {
                    action(null);
                    Debug.LogError(e);
                }
                catch (Exception e)
                {
                    action(null);
                    Debug.LogError("Unhandled exception:\n" + e);
                }
            }, error =>
            {
                action(null);
            });
        }

        public void UpdateMapProgress(MapProgress mapProgress, LevelInfo levelInfo, string playFabId)
        {
            mapProgress = UpdateMapProgress(mapProgress);
            if (mapProgress.Biomes[levelInfo.BiomeId].Progress == levelInfo.Progress)
            {
                mapProgress.Biomes[levelInfo.BiomeId].Progress++;
            }
            
            SendProgressToPlayFab(mapProgress, playFabId);
        }
        
        private void SendProgressToPlayFab(MapProgress progress, string playFabId)
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {PlayFabDataKeys.MapProgress, JsonConvert.SerializeObject(progress)}
                },
                PlayFabId = playFabId
            };
            
            PlayFabServerAPI.UpdateUserData(request, 
                result => Debug.Log("Map progress updated"), 
                error => Debug.LogError("Map progress update error:\n" + error
                ));
        }

        private MapProgress UpdateMapProgress(MapProgress mapProgress)
        {
            DateTime mapProgressTime = mapProgress.LastUpdated.ToUniversalTime();
            if (DateTime.UtcNow.DayOfYear != mapProgressTime.DayOfYear || DateTime.UtcNow.Year != mapProgressTime.Year)
            {
                mapProgress = new MapProgressBuilder().AddBiomes(3).Build();
            }
            mapProgress.LastUpdated = DateTime.UtcNow;
            return mapProgress;
        }
    }
}
#endif