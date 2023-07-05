using System;
using Core.Contracts;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cards;
using Core.Castle;
using Core.Match;
using Core.Match.Client;
using Core.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using MainMenu.Registration;
using Newtonsoft.Json;

namespace Core.Client
{
    [DisallowMultipleComponent]
    public class MatchClientController : MonoBehaviour
    {
        public static MatchClientController instance;
        public static UnityEvent matchWin;
        public static UnityEvent matchLose;
        public static UnityEvent matchDraw;

        public float timeSearchingLag;

        [Scene] public string battleScene;
        [Scene] public string menuScene;

        private Coroutine _searchingLag;

        private void Start()
        {
            instance = this;
            matchWin = new UnityEvent();
            matchLose = new UnityEvent();
            matchDraw = new UnityEvent();

            NetworkClient.RegisterHandler<MatchDetailsDto>(HandleMatchDetailsUpdate);
            NetworkClient.RegisterHandler<FatigueDto>(HandleFatigueUpdate);
            NetworkClient.RegisterHandler<LoadBattleSceneDto>(HandleLoadBattleScene);

            NetworkClient.RegisterHandler<RequestMatchDto>((requestMatchDto) =>
            {
                switch (requestMatchDto.RequestType)
                {
                    case MatchRequestType.FindingMatch:
                        PlayfabManager.TakeAwayEnergy(1);
                        BattleClientManager.ResetBattleClient();
                        GameScenesManager.LoadBattleScene();
                        break;
                    case MatchRequestType.WinMatch:
                        WinOnPoint();
                        matchWin.Invoke();
                        break;
                    case MatchRequestType.LoseMatch:
                        matchLose.Invoke();
                        break;
                    case MatchRequestType.DrawMatch:
                        matchDraw.Invoke();
                        break;
                    case MatchRequestType.EndTurn:
                        BattleClientManager.SetTurn(false);
                        break;
                    case MatchRequestType.StartTurn:
                        BattleClientManager.SetTurn(true);
                        break;
                    case MatchRequestType.ExitMatch:
                        //BattleClientManager.SetTurn(false);
                        break;
                }
            }, false);
        }

        private void WinOnPoint()
        {
            // if (ScensVar.LevelId != -1)
            // {
            //     char[] data = PlayerPrefs.GetString("Points").ToCharArray();
            //     data[ScensVar.LevelId] = '1';
            //     PlayerPrefs.SetString("Points", new string(data));
            //     PlayerPrefs.Save();
            // }
        }

        public static void EndTurn()
        {
            NetworkClientMiddleware.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.EndTurn
            });
        }

        public static void SearchingMatch()
        {
            if (MainClient.GetEnergyCount() > 0)
            {
                DebugManager.AddLineDebugText("Searching match...", nameof(SearchingMatch));

                if (instance._searchingLag != null)
                    instance.StopCoroutine(instance._searchingLag);

                instance._searchingLag = instance.StartCoroutine(instance.SearhicngLag());
            }
        }

        public static void SearchingBotMatch(int levelId)
        {
            //CORRECT ENERGY SPENDING
            if (MainClient.GetEnergyCount() > 0 /*&& bossType == 0*/)
            {
                DebugManager.AddLineDebugText("Searching match...", nameof(SearchingLagBot));

                if (instance._searchingLag != null)
                    instance.StopCoroutine(instance._searchingLag);

                instance._searchingLag = instance.StartCoroutine(instance.SearchingLagBot(levelId));
            }
            // else if(MainClient.GetEnergyCount() >= 0 && bossType != 0) 
            // {
            //     DebugManager.AddLineDebugText("Searching match...", nameof(SearchingLagBot));
            //
            //     if (instance._searchingLag != null)
            //         instance.StopCoroutine(instance._searchingLag);
            //
            //     instance._searchingLag = instance.StartCoroutine(instance.SearchingLagBot(bossType));
            //     //PlayfabManager.TakeAwayEnergy(4);
            // }
        }

        public static void CancelSearchingMatch()
        {
            DebugManager.RemoveLineDebugText(nameof(SearchingMatch));

            if (instance._searchingLag != null)
                instance.StopCoroutine(instance._searchingLag);

            NetworkClientMiddleware.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.CancelFindingMatch
            });
        }

        private IEnumerator SearhicngLag()
        {
            yield return new WaitForSeconds(timeSearchingLag);
            ScensVar.BossType = -1;
            ScensVar.LevelId = -1;
            NetworkClientMiddleware.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.FindingMatch,
                LevelId = -1
            });

            //HARDCODED
            if (BattleClientManager.instance != null)
                BattleClientManager.instance._timer = 45;
        }

        private IEnumerator SearchingLagBot(int levelId)
        {
            ScensVar.BossType = levelId;
            ScensVar.LevelId = levelId;
            NetworkClientMiddleware.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.FindingBotMatch,
                LevelId = levelId
            });

            yield break;
        }

        private void HandleMatchDetailsUpdate(MatchDetailsDto dto)
        {
            Debug.Log("DTO:\n" + JsonConvert.SerializeObject(dto));
            ClientMatchState matchState = BattleClientManager.instance.MatchState;
            if (matchState.IsMyTurn != dto.IsYourTurn)
                BattleClientManager.instance.OnTurnPassed();

            if (dto.CardsInHandIds != null)
            {
                List<Guid> guidsToRemove = new();
                foreach (var guid in matchState.DraftedCards)
                {
                    if (!dto.CardsInHandIds.Contains(guid.ToString()))
                        guidsToRemove.Add(guid);
                }

                foreach (var guid in guidsToRemove)
                {
                    matchState.RemoveDraftedCard(guid);
                }
            }

            if (matchState.MyState?.Castle != null)
            {
                matchState.OldMyCastle = new CastleEntity(matchState.MyState.Castle);
            }

            if (matchState.EnemyState?.Castle != null)
            {
                matchState.OldEnemyCastle = new CastleEntity(matchState.EnemyState.Castle);
            }

            MatchPlayerDto myDto = dto.Players.FirstOrDefault(p => p.PlayerId != "");
            MatchPlayerDto enemyDto = dto.Players.FirstOrDefault(p => p.PlayerId == "");
            MatchPlayer myState = matchState.MyState;
            MatchPlayer enemyState = matchState.EnemyState;
            myState.Castle = myDto.Castle;
            myState.Name = myDto.Name;
            myState.PlayFabId = myDto.PlayerId.ToString();
            enemyState.Castle = enemyDto.Castle;
            enemyState.Name = enemyDto.Name;
            matchState.IsMyTurn = dto.IsYourTurn;

            if (matchState.OldMyCastle != null)
            {
                matchState.OldMyCastle.Tower.MaxHealth = myDto.Castle.Tower.MaxHealth;
                matchState.OldMyCastle.Wall.MaxHealth = myDto.Castle.Wall.MaxHealth;
            }

            if (matchState.OldEnemyCastle != null)
            {
                matchState.OldEnemyCastle.Tower.MaxHealth = enemyDto.Castle.Tower.MaxHealth;
                matchState.OldEnemyCastle.Wall.MaxHealth = enemyDto.Castle.Wall.MaxHealth;
            }

            Guid[] cardsInHandIds = Array.Empty<Guid>();
            if (dto.CardsInHandIds != null)
                cardsInHandIds = dto.CardsInHandIds?.Select(Guid.Parse)?.ToArray();
            enemyState.PlayFabId = enemyDto.PlayerId.ToString();

            matchState.CardsInHandIds = cardsInHandIds;
            dto.Fatigue++;
            matchState.Fatigue = dto.Fatigue;
            matchState.LevelInfo = dto.LevelInfo;
            Debug.Log("MatchState after changes before apply:\n" + JsonConvert.SerializeObject(matchState));
            matchState.ApplyChanges();
        }

        private void HandleFatigueUpdate(FatigueDto dto)
        {
            BattleClientManager.instance.ApplyFatigue(dto.Damage);
        }

        private void HandleLoadBattleScene(LoadBattleSceneDto dto)
        {
            GameScenesManager.LoadBattleScene();
        }
    }
}