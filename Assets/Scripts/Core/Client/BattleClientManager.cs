using Core.Contracts;
using Core.Server;
using UnityEngine;
using Core.Castle;
using Core.Cards;
using System.Linq;
using System;
using System.Collections;
using Core.Match;
using Core.Match.Client;
using Core.Match.Server;
using Core.Utils;
using Effects;
using MainMenu.Registration;
using UnityEngine.SceneManagement;
using Mirror;
using Newtonsoft.Json;

namespace Core.Client
{
    public class BattleClientManager : MonoBehaviour
    {
        public static BattleClientManager instance;

        public ClientMatchState MatchState { get; } = new();
        private bool _canPlay;
        private bool _matchEnded;
        private float _timer;
        private float _elapsedTime;
        private int _winCount;
        private bool isFatigueEffectShowed = false;

        private int division;

        public static MatchPlayer GetMyData() => instance.MatchState.MyState;
        public static MatchPlayer GetEnemyData() => instance.MatchState.EnemyState;
        public static float GetTimeLeft() => instance._timer - instance._elapsedTime;
        public static bool IsMyTurn() => instance.MatchState.IsMyTurn;
        public static bool IsCanPlay() => instance._canPlay;
        public static bool IsMatchEnded() => instance._matchEnded;

        public static void ResetBattleClient()
        {
            instance.MatchState.Reset();
            instance.isFatigueEffectShowed = false;
            instance._canPlay = false;
            instance._matchEnded = false;
            instance._timer = 0;
            instance._elapsedTime = 0;
            //instance._damageFatigue = 0;
            //instance._fatigueLimit = 0;
        }

        public static void ResetTimer()
        {
            instance._elapsedTime = 0;
        }

        //Эта хуйня магическая и ее нужно переделать, neccesary to pay attention to this
        public static void SetTurn(bool isMyTurn)
        {
            if (CardObject.IsDiscardMode)
            {
                var cardObject = FindObjectsOfType<CardObject>()
                    .FirstOrDefault(c => c.card.NonDiscard == false);

                if (cardObject != null)
                    cardObject.Discard();

                SetCanPlay(false);

                CardObject.IsDiscardMode = false;
            }

            // if (SceneManager.GetActiveScene().name == "Battle")
            // {
            //     BattleUI.HideTipsWindow();
            //     
            //     if (instance._numberTurn % 2 == 0)
            //     {
            //         if (instance._numberTurn == instance._numberTurnForFatigue)
            //             BattleUI.ActivateFatigueDamageText();
            //
            //         if (instance._numberTurn == instance._numberTurnForFatigue)
            //             EffectSpawner.StartFatigueEffect();
            //
            //         if (instance._numberTurn >= instance._numberTurnForFatigue)
            //         {
            //             if (GetMyData().Castle.Wall.Health > 0)
            //                 BattleUI.DamageMyWall(instance._fatigue.Damage);
            //             else
            //                 BattleUI.DamageMyTower(instance._fatigue.Damage);
            //
            //             if (GetEnemyData().Castle.Wall.Health > 0)
            //                 BattleUI.DamageEnemyWall(instance._fatigue.Damage);
            //             else
            //                 BattleUI.DamageEnemyTower(instance._fatigue.Damage);
            //
            //             EffectSpawner.ApplyFatigueEffect();
            //
            //             instance._fatigue++;
            //         }
            //     }
            //
            //     BattleUI.SetTextFatigueDamage(instance._fatigue.Damage);
            // }
            
            if (true)
            {
                //instance._isMyTurn = isMyTurn;

                ResetTimer();
                SetCanPlay(isMyTurn);

                if (!isMyTurn && CardObject.IsDiscardMode)
                {
                    var cardObjects = FindObjectsOfType<CardObject>()
                        .Where(c => c.GetCardPosition() != null
                                    && !c.card.NonDiscard)
                        .ToList();

                    cardObjects.FirstOrDefault()!.OnEndDrag(null);
                }

                if (isMyTurn)
                {
                    BattleUI.SetTextMyTurn();
                    BattleUI.instanse.skipTurnButton.SetActive(true);
                }
                else
                {
                    BattleUI.SetTextEnemyTurn();
                    BattleUI.instanse.skipTurnButton.SetActive(false);
                }
            }
        }

        public static void SetCanPlay(bool canPlay)
        {
            instance._canPlay = canPlay;
        }

        // public bool CheckEndMatch()
        // {
        //     try
        //     {
        //         if (_enemyPlayerData.Castle.Tower.Health <= 0
        //            && _myPlayerData.Castle.Tower.Health <= 0)
        //         {
        //             BattleUI.instanse.ShowDrawWindow();
        //             _matchEnded = true;
        //
        //             return true;
        //         }
        //
        //         if (_myPlayerData.Castle.Tower.Health <= 0
        //             || _enemyPlayerData.Castle.Tower.Health >= _enemyPlayerData.Castle.Tower.MaxHealth)
        //         {
        //             BattleUI.instanse.ShowLoseWindow();
        //             _matchEnded = true;
        //
        //             return true;
        //         }
        //
        //         if (_enemyPlayerData.Castle.Tower.Health <= 0
        //             || _myPlayerData.Castle.Tower.Health >= _myPlayerData.Castle.Tower.MaxHealth)
        //         {
        //             BattleUI.instanse.ShowWinWindow();
        //             _matchEnded = true;
        //             return true;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         //TODO: Логгер сюда засунуть
        //         Debug.Log(e);
        //     }
        //
        //     return false;
        // }

        //Этот пиздец в будущем перенесется на сервер
        public void SetWin()
        {
            int _winCount = MainClient.GetWinCount();
            MainClient.SetWinCount(_winCount++);
            PlayfabManager.SendLeaderboard(_winCount);
        }

        private void Start()
        {
            instance = this;
            MatchState.OnStateChanged += OnMatchStateUpdated;

            NetworkClient.RegisterHandler<RequestBattleInfo>(requestBattleInfo =>
            {
                ResetBattleClient();

                Debug.Log($"RequestBattleInfo updated with division {requestBattleInfo.Division}!");

                division = requestBattleInfo.Division;

                ICastleCreator castleCreator = new DivisionCastleCreator(requestBattleInfo.Division);

                MatchPlayer myData = new()
                {
                    Name = requestBattleInfo.YourName,
                    Castle = castleCreator.CreateCastle()
                };

                MatchPlayer enemyData = new()
                {
                    Name = requestBattleInfo.EnemyName,
                    Castle = castleCreator.CreateCastle()
                };

                BattleUI.instanse.myNickname.text = MainClient.GetUsername();
                BattleUI.instanse.enemyNickname.text = requestBattleInfo.EnemyName;
                BattleUI.instanse.textHealthMyTower.text = requestBattleInfo.YourTowerHealth.ToString();
                BattleUI.instanse.textHealthEnemyTower.text = requestBattleInfo.EnemyTowerHealth.ToString();
                BattleUI.instanse.textHealthMyWall.text = requestBattleInfo.YourWallHealth.ToString();
                BattleUI.instanse.textHealthEnemyWall.text = requestBattleInfo.EnemyWallHealth.ToString();
                BattleUI.instanse.enemyResourceIncome_1.text =
                    enemyData.Castle.GetResource("Resource_1").Income.ToString();
                BattleUI.instanse.enemyResourceIncome_2.text =
                    enemyData.Castle.GetResource("Resource_2").Income.ToString();
                BattleUI.instanse.enemyResourceIncome_3.text =
                    enemyData.Castle.GetResource("Resource_3").Income.ToString();
                BattleUI.instanse.myResourceIncome_1.text = myData.Castle.GetResource("Resource_1").Income.ToString();
                BattleUI.instanse.myResourceIncome_2.text = myData.Castle.GetResource("Resource_2").Income.ToString();
                BattleUI.instanse.myResourceIncome_3.text = myData.Castle.GetResource("Resource_3").Income.ToString();
                BattleUI.instanse.enemyResourceValue_1.text =
                    enemyData.Castle.GetResource("Resource_1").Value.ToString();
                BattleUI.instanse.enemyResourceValue_2.text =
                    enemyData.Castle.GetResource("Resource_2").Value.ToString();
                BattleUI.instanse.enemyResourceValue_3.text =
                    enemyData.Castle.GetResource("Resource_3").Value.ToString();
                BattleUI.instanse.myResourceValue_1.text = myData.Castle.GetResource("Resource_1").Value.ToString();
                BattleUI.instanse.myResourceValue_2.text = myData.Castle.GetResource("Resource_2").Value.ToString();
                BattleUI.instanse.myResourceValue_3.text = myData.Castle.GetResource("Resource_3").Value.ToString();
                BattleUI.instanse.enemyWinCountText.text = requestBattleInfo.EnemyWinCount.ToString();

                _timer = requestBattleInfo.Timer;
                _matchEnded = false;

                BattleUI.HideWaitStartWindow();
                SetTurn(requestBattleInfo.IsYourTurn);
            }, false);
        }

        private void FixedUpdate()
        {
            _elapsedTime += Time.fixedDeltaTime;

            if (_elapsedTime > _timer)
                _elapsedTime = _timer;
        }

        public void ApplyFatigue(int damage)
        {
            if (SceneManager.GetActiveScene().name != "Battle")
                return;

            BattleUI.HideTipsWindow();

            if (!isFatigueEffectShowed)
            {
                BattleUI.ActivateFatigueDamageText();
                EffectSpawner.StartFatigueEffect();
            }

            isFatigueEffectShowed = true;

            if (GetMyData().Castle.Wall.Health > 0)
                BattleUI.DamageMyWall(damage);
            else
                BattleUI.DamageMyTower(damage);

            if (GetEnemyData().Castle.Wall.Health > 0)
                BattleUI.DamageEnemyWall(damage);
            else
                BattleUI.DamageEnemyTower(damage);

            EffectSpawner.ApplyFatigueEffect();

            BattleUI.SetTextFatigueDamage(damage);
        }


        private void OnMatchStateUpdated(ClientMatchState state)
        {
            if (SceneManager.GetActiveScene().name != MatchClientController.instance.battleScene)
            {
                AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(MatchClientController.instance.battleScene);
                asyncOperation.completed += (op) => StartCoroutine(UpdateUIFromState(state, true));
                return;
            }
            StartCoroutine(UpdateUIFromState(state, false));
        }
        
        //Куратинный кастылек тк если обновлять данные сразу после загрузки сцены есть подозрение
        //что они перезаписываются на стандартные задданые в сцене и вообще сцена не реагирует на скрипты
        private IEnumerator UpdateUIFromState(ClientMatchState state, bool wait)
        {
            if(wait)
                yield return new WaitForSeconds(1);
            
            BattleUI.instanse.myNickname.text = state.MyState.Name;
            BattleUI.instanse.enemyNickname.text = state.EnemyState.Name;
            BattleUI.instanse.textHealthMyTower.text = state.MyState.Castle.Tower.Health.ToString();
            BattleUI.instanse.textHealthEnemyTower.text = state.EnemyState.Castle.Tower.Health.ToString();
            BattleUI.instanse.textHealthMyWall.text = state.MyState.Castle.Wall.Health.ToString();
            BattleUI.instanse.textHealthEnemyWall.text = state.EnemyState.Castle.Wall.Health.ToString();
            BattleUI.instanse.enemyResourceIncome_1.text =
                state.EnemyState.Castle.GetResource("Resource_1").Income.ToString();
            BattleUI.instanse.enemyResourceIncome_2.text =
                state.EnemyState.Castle.GetResource("Resource_2").Income.ToString();
            BattleUI.instanse.enemyResourceIncome_3.text =
                state.EnemyState.Castle.GetResource("Resource_3").Income.ToString();
            BattleUI.instanse.myResourceIncome_1.text = state.MyState.Castle.GetResource("Resource_1").Income.ToString();
            BattleUI.instanse.myResourceIncome_2.text = state.MyState.Castle.GetResource("Resource_2").Income.ToString();
            BattleUI.instanse.myResourceIncome_3.text = state.MyState.Castle.GetResource("Resource_3").Income.ToString();
            BattleUI.instanse.enemyResourceValue_1.text =
                state.EnemyState.Castle.GetResource("Resource_1").Value.ToString();
            BattleUI.instanse.enemyResourceValue_2.text =
                state.EnemyState.Castle.GetResource("Resource_2").Value.ToString();
            BattleUI.instanse.enemyResourceValue_3.text =
                state.EnemyState.Castle.GetResource("Resource_3").Value.ToString();
            BattleUI.instanse.myResourceValue_1.text = state.MyState.Castle.GetResource("Resource_1").Value.ToString();
            BattleUI.instanse.myResourceValue_2.text = state.MyState.Castle.GetResource("Resource_2").Value.ToString();
            BattleUI.instanse.myResourceValue_3.text = state.MyState.Castle.GetResource("Resource_3").Value.ToString();
            foreach (var guid in state.CardsToDraftGuids)
            {
                CardSpawner.SpawnDraftCard(guid);
            }
            //BattleUI.instanse.enemyWinCountText.text = state.MyState.;
            
            _matchEnded = false;

            BattleUI.HideWaitStartWindow();
            SetTurn(state.IsMyTurn);
        }
    }
}