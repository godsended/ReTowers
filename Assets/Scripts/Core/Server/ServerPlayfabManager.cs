using System.Collections.Generic;
using UnityEngine;
using PlayFab.ServerModels;
using PlayFab;
using System;
using Core.Cards;
using Newtonsoft.Json;
using System.Linq;
using Core.Map;
using Core.Map.Server;

namespace Core.Server
{
    public class ServerPlayfabManager : MonoBehaviour
    {
        public static ServerPlayfabManager instance;

        void Awake()
        {
            instance = this;
        }

        public void SetNewPlayer(PlayerData player)
        {
            CheckGetDailyEnergy(player);
        }

        private void CheckGetDailyEnergy(PlayerData player)
        {
            if (player.LastLoginTime.Date.AddHours(3) != DateTime.UtcNow.Date.AddHours(3))
            {
                SetEnergy(player, 20);
            }
        }

        private void SetEnergy(PlayerData player, int value)
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                PlayFabId = player.PlayFabId,

                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "Energy",
                        Value = value
                    }
                }
            };
            PlayFabServerAPI.UpdatePlayerStatistics(request, null, null);
        }

        private List<PlayerData> players = new List<PlayerData>();

        public void GetPlayerStatistic(PlayerData player)
        {
            players.Add(player);
            List<string> statNames = new List<string>() {"WinsCount"};
            var request = new GetPlayerStatisticsRequest
            {
                StatisticNames = statNames,
                PlayFabId = player.PlayFabId
            };
            PlayFabServerAPI.GetPlayerStatistics(request, OnPlayerStatisticRecieved, null);
        }

        private void OnPlayerStatisticRecieved(GetPlayerStatisticsResult result)
        {
            foreach (var player in players)
            {
                if (player.PlayFabId == result.PlayFabId)
                {
                    if (result.Statistics.Count > 0)
                    {
                        player.PlayerStatistics.WinCount = result.Statistics[0].Value;
                    }

                    players.Remove(player);
                    break;
                }
            }
        }

        public void GetUserData(PlayerData player, bool isBotMatch, int levelId = -1)
        {
            players.Add(player);
            var request = new GetUserDataRequest
            {
                PlayFabId = player.PlayFabId
            };
            if (!isBotMatch)
                PlayFabServerAPI.GetUserData(request, OnPlayerDataRecieved, null);
            else
            {
                PlayFabServerAPI.GetUserData(request, OnPlayerDataRecievedInBotMatch, null, levelId);
            }
        }

        private void OnPlayerDataRecieved(GetUserDataResult result)
        {
            foreach (var player in players)
            {
                if (player.PlayFabId == result.PlayFabId)
                {
                    if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
                    {
                        List<CardJson> cards =
                            JsonConvert.DeserializeObject<List<CardJson>>(result.Data["PlayerCards"].Value);
                        List<CardData> cardDatas = LibraryCards.GetPlayerCards();
                        List<CardData> DeckDatas = new List<CardData>();
                        foreach (var card in cards)
                        {
                            for (int i = 0; i < cardDatas.Count; i++)
                            {
                                if (cardDatas[i].Id == card.Id && card.InDeck)
                                {
                                    DeckDatas.Add(cardDatas[i]);
                                    cardDatas[i].InDeck = true;
                                    break;
                                }
                            }
                        }

                        player.Cards = new PlayerCards(DeckDatas.Select(c => Guid.Parse(c.Id)).ToList());

                        player.Division = DivisionCalculator.CalculateDivision(DeckDatas.ToArray());

                        MatchServerController.AddPlayerLookingMatch(player);
                        players.Remove(player);
                        return;
                    }
                }
            }
        }

        private void OnPlayerDataRecievedInBotMatch(GetUserDataResult result)
        {
            foreach (var player in players)
            {
                if (player.PlayFabId != result.PlayFabId) continue;

                if (result.Data == null || !result.Data.ContainsKey("PlayerCards")) continue;

                ServerMapController.Instance.LoadMapProgress(result.PlayFabId, progress =>
                {
                    LevelInfo level =
                        ServerMapController.Instance.LevelsConfiguration.Levels.FirstOrDefault(l =>
                            l.LevelId == (int) result.CustomData);

                    if (level == null)
                        return;

                    if (level.Progress > progress.Biomes[level.BiomeId].Progress)
                        return;

                    List<CardJson> cards =
                        JsonConvert.DeserializeObject<List<CardJson>>(result.Data["PlayerCards"].Value);
                    List<CardData> cardDatas = LibraryCards.GetPlayerCards();
                    List<CardData> DeckDatas = new List<CardData>();
                    for (int i = 0; i < cardDatas.Count; i++)
                    {
                        if (cardDatas[i].Rang == 0)
                        {
                            DeckDatas.Add(cardDatas[i]);
                            break;
                        }
                    }

                    player.Division = DivisionCalculator.CalculateDivision(DeckDatas.ToArray());
                    player.Cards = new PlayerCards(DeckDatas.Select(c => Guid.Parse(c.Id)).ToList());
                    Debug.Log($"WhenPlayerDataRecievedInBotMatch player division {player.Division}");
                    MatchServerController.instance.StartToBot(level, progress, player);
                    players.Remove(player);
                });
            }
        }
    }
}