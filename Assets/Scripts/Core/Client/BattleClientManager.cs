using Core.Contracts;
using Core.Server;
using UnityEngine;
using Core.Castle;
using Core.Cards;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Core.Match;
using Core.Match.Client;
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
        public float _timer = 45;
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
            if (instance != null)
            {
                instance.MatchState.Reset();
                instance.isFatigueEffectShowed = false;
                instance._canPlay = false;
                instance._matchEnded = false;
                instance._timer = 0;
                instance._elapsedTime = 0;
            }
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
                    BattleUI.Instance.skipTurnButton.SetActive(true);
                }
                else
                {
                    BattleUI.SetTextEnemyTurn();
                    BattleUI.Instance.skipTurnButton.SetActive(false);
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
        //             BattleUI.Instance.ShowDrawWindow();
        //             _matchEnded = true;
        //
        //             return true;
        //         }
        //
        //         if (_myPlayerData.Castle.Tower.Health <= 0
        //             || _enemyPlayerData.Castle.Tower.Health >= _enemyPlayerData.Castle.Tower.MaxHealth)
        //         {
        //             BattleUI.Instance.ShowLoseWindow();
        //             _matchEnded = true;
        //
        //             return true;
        //         }
        //
        //         if (_enemyPlayerData.Castle.Tower.Health <= 0
        //             || _myPlayerData.Castle.Tower.Health >= _myPlayerData.Castle.Tower.MaxHealth)
        //         {
        //             BattleUI.Instance.ShowWinWindow();
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
            MatchState.OnStateChanged -= OnMatchStateUpdated;
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

                BattleUI.Instance.myNickname.text = MainClient.GetUsername();
                BattleUI.Instance.enemyNickname.text = requestBattleInfo.EnemyName;
                BattleUI.Instance.textHealthMyTower.text = requestBattleInfo.YourTowerHealth.ToString();
                BattleUI.Instance.textHealthEnemyTower.text = requestBattleInfo.EnemyTowerHealth.ToString();
                BattleUI.Instance.textHealthMyWall.text = requestBattleInfo.YourWallHealth.ToString();
                BattleUI.Instance.textHealthEnemyWall.text = requestBattleInfo.EnemyWallHealth.ToString();
                BattleUI.Instance.enemyResourceIncome_1.text =
                    enemyData.Castle.GetResource("Resource_1").Income.ToString();
                BattleUI.Instance.enemyResourceIncome_2.text =
                    enemyData.Castle.GetResource("Resource_2").Income.ToString();
                BattleUI.Instance.enemyResourceIncome_3.text =
                    enemyData.Castle.GetResource("Resource_3").Income.ToString();
                BattleUI.Instance.myResourceIncome_1.text = myData.Castle.GetResource("Resource_1").Income.ToString();
                BattleUI.Instance.myResourceIncome_2.text = myData.Castle.GetResource("Resource_2").Income.ToString();
                BattleUI.Instance.myResourceIncome_3.text = myData.Castle.GetResource("Resource_3").Income.ToString();
                BattleUI.Instance.enemyResourceValue_1.text =
                    enemyData.Castle.GetResource("Resource_1").Value.ToString();
                BattleUI.Instance.enemyResourceValue_2.text =
                    enemyData.Castle.GetResource("Resource_2").Value.ToString();
                BattleUI.Instance.enemyResourceValue_3.text =
                    enemyData.Castle.GetResource("Resource_3").Value.ToString();
                BattleUI.Instance.myResourceValue_1.text = myData.Castle.GetResource("Resource_1").Value.ToString();
                BattleUI.Instance.myResourceValue_2.text = myData.Castle.GetResource("Resource_2").Value.ToString();
                BattleUI.Instance.myResourceValue_3.text = myData.Castle.GetResource("Resource_3").Value.ToString();
                BattleUI.Instance.enemyWinCountText.text = requestBattleInfo.EnemyWinCount.ToString();

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

        public void ApplyFatigue(int nextDamage)
        {
            BattleUI.HideTipsWindow();

            if (!isFatigueEffectShowed)
            {
                BattleUI.ActivateFatigueDamageText();
                EffectSpawner.StartFatigueEffect();
            }

            isFatigueEffectShowed = true;

            // if (GetMyData().Castle.Wall.Health > 0)
            //     BattleUI.DamageMyWall(damage);
            // else
            //     BattleUI.DamageMyTower(damage);
            //
            // if (GetEnemyData().Castle.Wall.Health > 0)
            //     BattleUI.DamageEnemyWall(damage);
            // else
            //     BattleUI.DamageEnemyTower(damage);

            EffectSpawner.ApplyFatigueEffect();

            BattleUI.SetTextFatigueDamage(nextDamage);
        }


        private void OnMatchStateUpdated(ClientMatchState state, CastleEntity newMyCastle, CastleEntity newEnemyCastle)
        {
            // if (SceneManager.GetActiveScene().path != MatchClientController.instance.battleScene)
            // {
            //     Debug.Log("Loading battle scene");
            //     AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(MatchClientController.instance.battleScene);
            //     asyncOperation.completed += (op) =>
            //         StartCoroutine(UpdateUIFromState(state, true, newMyCastle, newEnemyCastle));
            //     return;
            // }

            StartCoroutine(UpdateUIFromState(state, newMyCastle, newEnemyCastle));
        }

        //Куратинный кастылек тк если обновлять данные сразу после загрузки сцены есть подозрение
        //что они перезаписываются на стандартные задданые в сцене и вообще сцена не реагирует на скрипты
        private IEnumerator UpdateUIFromState(ClientMatchState state, CastleEntity newMyCastle,
            CastleEntity newEnemyCastle)
        {
            Debug.Log("State after reply before rollback\n" + JsonConvert.SerializeObject(state));
            Debug.Log("newMyCastle before rollback\n" + JsonConvert.SerializeObject(newMyCastle));
            state.RollbackCastles();
            Debug.Log("newMyCastle after rollback\n" + JsonConvert.SerializeObject(newMyCastle));
            Debug.Log("State after reply after rollback\n" + JsonConvert.SerializeObject(state));
            yield return new WaitForSeconds(0.3f);

            BattleUI.Instance.myNickname.text = state.MyState.Name;
            BattleUI.Instance.enemyNickname.text = state.EnemyState.Name;

            int change =
                newMyCastle.Tower.Health - state.OldMyCastle.Tower.Health;
            if (change > 0)
            {
                BattleUI.HealMyTower(change);
                Debug.Log($"Хилимся на {change}");
            }
            else if (change < 0)
                BattleUI.DamageMyTower(-change);

            change =
                newEnemyCastle.Tower.Health - state.OldEnemyCastle.Tower.Health;
            if (change > 0)
                BattleUI.HealEnemyTower(change);
            else if (change < 0)
                BattleUI.DamageEnemyTower(-change);

            change =
                newMyCastle.Wall.Health - state.OldMyCastle.Wall.Health;
            if (change > 0)
                BattleUI.HealMyWall(change);
            else if (change < 0)
                BattleUI.DamageMyWall(-change);

            change =
                newEnemyCastle.Wall.Health - state.OldEnemyCastle.Wall.Health;
            if (change > 0)
                BattleUI.HealEnemyWall(change);
            else if (change < 0)
                BattleUI.DamageEnemyWall(-change);

            change =
                newEnemyCastle.GetResource("Resource_1").Income -
                int.Parse(BattleUI.Instance.enemyResourceIncome_1.text);
            if (change > 0)
                BattleUI.AddEnemyResourceIncome("Resource_1", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceIncome("Resource_1", -change);

            change =
                newEnemyCastle.GetResource("Resource_2").Income -
                int.Parse(BattleUI.Instance.enemyResourceIncome_2.text);
            if (change > 0)
                BattleUI.AddEnemyResourceIncome("Resource_2", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceIncome("Resource_2", -change);

            change =
                newEnemyCastle.GetResource("Resource_3").Income -
                int.Parse(BattleUI.Instance.enemyResourceIncome_3.text);
            if (change > 0)
                BattleUI.AddEnemyResourceIncome("Resource_3", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceIncome("Resource_3", -change);

            change =
                newEnemyCastle.GetResource("Resource_1").Value - int.Parse(BattleUI.Instance.enemyResourceValue_1.text);
            if (change > 0)
                BattleUI.AddEnemyResourceValue("Resource_1", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceValue("Resource_1", -change);

            change =
                newEnemyCastle.GetResource("Resource_2").Value - int.Parse(BattleUI.Instance.enemyResourceValue_2.text);
            if (change > 0)
                BattleUI.AddEnemyResourceValue("Resource_2", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceValue("Resource_2", -change);

            change =
                newEnemyCastle.GetResource("Resource_3").Value - int.Parse(BattleUI.Instance.enemyResourceValue_3.text);
            if (change > 0)
                BattleUI.AddEnemyResourceValue("Resource_3", change);
            else if (change < 0)
                BattleUI.RemoveEnemyResourceValue("Resource_3", -change);

            change =
                newMyCastle.GetResource("Resource_1").Income - int.Parse(BattleUI.Instance.myResourceIncome_1.text);
            if (change > 0)
                BattleUI.AddMyResourceIncome("Resource_1", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceIncome("Resource_1", -change);

            change =
                newMyCastle.GetResource("Resource_2").Income - int.Parse(BattleUI.Instance.myResourceIncome_2.text);
            if (change > 0)
                BattleUI.AddMyResourceIncome("Resource_2", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceIncome("Resource_2", -change);

            change =
                newMyCastle.GetResource("Resource_3").Income - int.Parse(BattleUI.Instance.myResourceIncome_3.text);
            if (change > 0)
                BattleUI.AddMyResourceIncome("Resource_3", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceIncome("Resource_3", -change);

            change =
                newMyCastle.GetResource("Resource_1").Value - int.Parse(BattleUI.Instance.myResourceValue_1.text);
            if (change > 0)
                BattleUI.AddMyResourceValue("Resource_1", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceValue("Resource_1", -change);

            change =
                newMyCastle.GetResource("Resource_2").Value - int.Parse(BattleUI.Instance.myResourceValue_2.text);
            if (change > 0)
                BattleUI.AddMyResourceValue("Resource_2", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceValue("Resource_2", -change);

            change =
                newMyCastle.GetResource("Resource_3").Value - int.Parse(BattleUI.Instance.myResourceValue_3.text);
            if (change > 0)
                BattleUI.AddMyResourceValue("Resource_3", change);
            else if (change < 0)
                BattleUI.RemoveMyResourceValue("Resource_3", -change);

            Debug.Log("State after UI changes\n" + JsonConvert.SerializeObject(state));
            var cardsPositions = FindObjectsOfType<CardPosition>();

            List<GameObject> cardsToDestroy = new List<GameObject>();
            foreach (var cp in cardsPositions)
            {
                if (cp.card?.card == null || state.DraftedCards.Contains(Guid.Parse(cp.card.card.Id))) 
                    continue;
                cardsToDestroy.Add(cp.card.gameObject);
                cp.card = null;
            }

            foreach (var card in cardsToDestroy)
            {
                Destroy(card);
            }
            
            foreach (var id in state.CardsInHandIds!)
            {
                if (state.DraftedCards.Contains(id) || cardsPositions.Any(c => (c?.card?.card?.Id ?? "") == id.ToString()))
                    continue;
                state.AddDraftedCard(id);
                Debug.Log("Draft card!");
            }

            Debug.Log("State after card spawning\n" + JsonConvert.SerializeObject(state));
            //BattleUI.Instance.enemyWinCountText.text = state.MyState.;

            _matchEnded = false;

            // if (wait)
            //     BattleUI.HideWaitStartWindow();

            SetTurn(state.IsMyTurn);
        }

        public void OnTurnPassed()
        {
            BattleUI.HideTipsWindow();
            ResetTimer();
            //ЗАХАРДКОЖЕНО, ИСПРАВИТЬ
            _timer = 30;
            //DraftLoosedCards();
        }

        // private void DraftLoosedCards()
        // {
        //     var cardDatas = FindObjectsOfType<CardPosition>()?.Select(c => c?.card?.card)
        //         ?? Array.Empty<CardData>();
        //     var cardIds = cardDatas?.Select(c => c?.Id) ?? Array.Empty<string>();
        //
        //     foreach (var guid in MatchState.CardsInHandIds)
        //     {
        //         if (cardIds.Contains(guid.ToString())) 
        //             continue;
        //         MatchState.RemoveDraftedCard(guid);
        //         MatchState.AddDraftedCard(guid);
        //     }
        // }
    }
}