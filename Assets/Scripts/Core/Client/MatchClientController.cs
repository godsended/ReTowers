using Core.Contracts;
using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using MainMenu.Registration;

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

        [Scene]
        public string battleScene;
        [Scene]
        public string menuScene;

        private Coroutine _searchingLag;

        private void Start()
        {
            instance = this;
            matchWin = new UnityEvent();
            matchLose = new UnityEvent();
            matchDraw = new UnityEvent();

            NetworkClient.RegisterHandler<RequestMatchDto>((requestMatchDto) => 
            {
                switch (requestMatchDto.RequestType)
                {
                    case MatchRequestType.FindingMatch:
                        PlayfabManager.TakeAwayEnergy(1);
                        SceneManager.LoadScene(battleScene);
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
            if(ScensVar.LevelId != -1) 
            {
                char[] data = PlayerPrefs.GetString("Points").ToCharArray();
                data[ScensVar.LevelId] = '1';
                PlayerPrefs.SetString("Points", new string(data));
                PlayerPrefs.Save();
            }
        }

        public static void EndTurn()
        {
            NetworkClient.Send(new RequestMatchDto
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

            NetworkClient.Send(new RequestMatchDto
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
            NetworkClient.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.FindingMatch,
                LevelId = -1
            });
        }

        private IEnumerator SearchingLagBot(int levelId)
        {
            ScensVar.BossType = levelId;
            ScensVar.LevelId = levelId;
            NetworkClient.Send(new RequestMatchDto
            {
                AccountId = MainClient.GetClientId(),
                RequestType = MatchRequestType.FindingBotMatch,
                LevelId = levelId
            });

            yield break;
        }
    }
}