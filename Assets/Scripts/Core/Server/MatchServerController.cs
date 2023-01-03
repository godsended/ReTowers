using Core.Contracts;
using Core.Logging;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Timers;
using Core.Utils;
using System.Linq;

namespace Core.Server
{
    /// <summary>
    /// Match controller
    /// Responsible for the logic of finding a match among players
    /// </summary>
    [DisallowMultipleComponent]
    public class MatchServerController : MonoBehaviour
    {
        public int TimeToStartBot = 45;
        
        public static MatchServerController instance;

        /// <summary>
        /// Number of players in a match
        /// Default: 2
        /// </summary>
        public int MatchSize { get; set; } = 2;

        private List<PlayerData> _playersLookingForMatches;
        private List<Match> _matches { get; set; }
        private IGameLogger _gameLogger;
        private Timer _timer;

        [SerializeField]private List<int> _times;
        
        /// <summary>
        /// Add a player who is looking for a match
        /// </summary>
        /// <param name="player">Looking player</param>
        public static void RemoveMatch(Guid matchId)
        {
            Match match = instance._matches.FirstOrDefault(m => m.Id == matchId);

            instance._matches.Remove(match);
        }

        /// <summary>
        /// Add a player who is looking for a match
        /// </summary>
        /// <param name="player">Looking player</param>
        public static void AddPlayerLookingMatch(PlayerData player)
        {
            if (!instance._playersLookingForMatches.Contains(player))
            {
                var match = instance._matches
                  .Where(m => m.Players
                      .FirstOrDefault(p => p.Key.Id == player.Id).Key != null)
                  .FirstOrDefault();

                if (match != null)
                    match.Players.Remove(player);

                instance._playersLookingForMatches.Add(player);
                instance.StartCoroutine(instance.CheckMatching());

                instance._gameLogger.Log($"[{player.Name} ({player.Id})] add looking for match!", LogTypeMessage.Low);
                DebugManager.AddLineDebugText($"Player searching match: {instance._playersLookingForMatches.Count}", "PlayersLookingForMatches");
            }
        }

        /// <summary>
        /// Remove a player who is looking for a match
        /// </summary>
        /// <param name="player">Looking player</param>
        public static void RemovePlayerLookingMatch(PlayerData player)
        {
            instance._playersLookingForMatches.Remove(player);

            instance._gameLogger.Log($"[{player.Name} ({player.Id})] remove looking for match!", LogTypeMessage.Low);
            DebugManager.AddLineDebugText($"Player searching match: {instance._playersLookingForMatches.Count}", "PlayersLookingForMatches");
        }

        private void Awake()
        {
            instance = this;

            _playersLookingForMatches = new List<PlayerData>();
            _matches = new List<Match>();
            _gameLogger = new ConsoleLogger(new List<LogTypeMessage>
            {
                LogTypeMessage.Info,
                LogTypeMessage.Warning,
                LogTypeMessage.Error,
                LogTypeMessage.Low
            });
        }

        private void Start()
        {
            InitTimer();
            
            TowerSmashNetwork.ServerOnDisconnectEvent.AddListener((serverConnect) =>
            {
                var match = _matches
                    .Where(m => m.Players
                        .FirstOrDefault(p => p.Key.Connection == serverConnect).Key != null)
                    .FirstOrDefault();

                if (match != null)
                {
                    foreach (PlayerData player in match.Players.Keys)
                    {
                        if (player != null && player.Connection != null)
                        {
                            player.Connection.Send(new RequestMatchDto
                            {
                                RequestType = MatchRequestType.WinMatch
                            });
                        }
                    }

                    match.Stop();
                }
            });

            NetworkServer.RegisterHandler<RequestBattleInfo>((connection, requestBattleDto) =>
            {
                var match = _matches
                    .Where(m => m.Players
                        .FirstOrDefault(p => p.Key.Id == requestBattleDto.AccountId).Key != null)
                    .FirstOrDefault();

                if (match != null)
                {
                    var yourData = match.Players.FirstOrDefault(p => p.Key.Id == requestBattleDto.AccountId).Key;
                    var enemyData = match.Players.FirstOrDefault(p => p.Key.Id != requestBattleDto.AccountId).Key;
                    
                    match.MatchDivision = yourData.Division;
                    if (enemyData != null)
                        match.MatchDivision = Math.Min(match.MatchDivision, enemyData.Division);

                    DivisionCastleCreator castleCreator = new DivisionCastleCreator(match.MatchDivision);
                    MatchPlayerDataInitializer<DivisionCastleCreator> playerDataInitializer = new(castleCreator);
                    playerDataInitializer.Initialize(yourData);

                    if (enemyData != null)
                    {
                        playerDataInitializer.Initialize(enemyData);
                        Debug.Log($"Sending ReqBattleInfo with division {match.MatchDivision}");
                        connection.Send(new RequestBattleInfo
                        {
                            AccountId = requestBattleDto.AccountId,
                            YourName = yourData.Name,
                            EnemyName = enemyData.Name,
                            YourTowerHealth = yourData.Castle.Tower.Health,
                            EnemyTowerHealth = enemyData.Castle.Tower.Health,
                            YourWallHealth = yourData.Castle.Wall.Health,
                            EnemyWallHealth = enemyData.Castle.Wall.Health,
                            IsYourTurn = match.CurrentPlayerTurn.Id == requestBattleDto.AccountId,
                            Timer = match.TurnTime,
                            EnemyWinCount = enemyData.PlayerStatistics.WinCount,
                            StartDamageFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueDamageStart"]),
                            TurnFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]),
                            FatigueLimit = int.Parse(Configurator.data["BattleConfiguration"]["fatigueLimit"]),
                            Division = match.MatchDivision
                        });

                        match.PlayerReady(requestBattleDto.AccountId);
                    }
                    else 
                    {

                        string[] names = File.ReadAllLines(Application.dataPath + "/StreamingAssets" + "/names.txt");


                        connection.Send(new RequestBattleInfo
                        {
                            AccountId = requestBattleDto.AccountId,
                            YourName = yourData.Name,
                            EnemyName = names[UnityEngine.Random.Range(0, names.Length)],
                            YourTowerHealth = yourData.Castle.Tower.Health,
                            EnemyTowerHealth = yourData.Castle.Tower.Health,
                            YourWallHealth = yourData.Castle.Wall.Health,
                            EnemyWallHealth = yourData.Castle.Wall.Health,
                            IsYourTurn = match.CurrentPlayerTurn.Id == requestBattleDto.AccountId,
                            Timer = match.TurnTime,
                            EnemyWinCount = UnityEngine.Random.Range(yourData.PlayerStatistics.WinCount, yourData.PlayerStatistics.WinCount + 5),
                            StartDamageFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueDamageStart"]),
                            TurnFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]),
                            FatigueLimit = int.Parse(Configurator.data["BattleConfiguration"]["fatigueLimit"]),
                            Division = match.MatchDivision
                        });
                        match.PlayerReady(requestBattleDto.AccountId);
                    }
                }
                else
                {
                    _gameLogger.Log($"Match, contains player {requestBattleDto.AccountId} not founded!", LogTypeMessage.Low);
                }
            }, false);

            NetworkServer.RegisterHandler<RequestMatchDto>((connection, requestMatchDto) =>
            {
                PlayerData playerData = MainServer.GetPlayerData(requestMatchDto.AccountId);

                if (playerData != null)
                {
                    switch (requestMatchDto.RequestType)
                    {
                        case MatchRequestType.FindingMatch:
                            ServerPlayfabManager.instance.GetUserData(playerData, false);
                            break;
                        case MatchRequestType.FindingBotMatch:
                            ServerPlayfabManager.instance.GetUserData(playerData, true);
                            break;
                        case MatchRequestType.CancelFindingMatch:
                            for (int i = 0; i < instance._playersLookingForMatches.Count; i++)
                            {
                                if(playerData.Id == instance._playersLookingForMatches[i].Id) 
                                {
                                    StopTimer(i);
                                    break;
                                }
                            }
                            RemovePlayerLookingMatch(playerData);
                            break;
                        case MatchRequestType.ExitMatch:
                            LeaveMatch(playerData);
                            break;
                        case MatchRequestType.EndTurn:
                            RequestEndTurn(playerData);
                            break;
                    }
                }
                else
                {
                    _gameLogger.Log($"Player data is not found!", LogTypeMessage.Low);
                }
            }, false);
        }

        private void RequestEndTurn(PlayerData player)
        {
            var match = _matches
                    .Where(m => m.Players
                        .FirstOrDefault(p => p.Key.Id == player.Id).Key != null)
                    .FirstOrDefault();

            if (match != null)
                if (match.CurrentPlayerTurn == player)
                    StartCoroutine(match.WaitNextTurn());
        }

        private void LeaveMatch(PlayerData player)
        {
            var match = _matches
                    .Where(m => m.Players
                        .FirstOrDefault(p => p.Key.Id == player.Id).Key != null)
                    .FirstOrDefault();

            if (match != null)
            {
                player.Castle.Tower.Damage(player.Castle.Tower.Health);
                if (match.Players.Count == 1)
                {
                    match.Stop();
                }
                else
                {
                    match.CheckEndMatch();
                }
            }
        }

        private void InitTimer()
        {
            _timer = new Timer(1000);

            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            for(int i = 0; i < _times.Count; i++)
            {
                _times[i]--;

                if (_times[i] <= 0)
                {
                    StopTimer(i);
                    Debug.Log($"TimerOnEllapsed player division {_playersLookingForMatches[i].Division}");
                    StartToBot(_playersLookingForMatches[i]);
                }
            }
        }

        private void StartTimer()
        {
            _times.Add(TimeToStartBot);
            
            Debug.Log("Here we start");
            _timer.Start();
            Debug.Log("Here we end");
        }
        
        private void StopTimer(int i)
        {
            _times.Remove(_times[i]);

            if(_times.Count == 0) 
            {
                _timer.Stop();
            }
        }

        public void StartToBot(PlayerData player = null)
        {
            if (player == null)
            {
                player = instance._playersLookingForMatches.First();
            }

            _matches.Add(new Match(player, _gameLogger));

            StartMatch(player);
        }
        
        private IEnumerator CheckMatching()
        {
            if (instance._playersLookingForMatches.Count <= 0)
                yield break;

            int playerindex = -1;
            int playersCount = instance._playersLookingForMatches.Count;

            for (int i = 0; i < playersCount - 1; i++)
            {
                if (instance._playersLookingForMatches[i].Division == instance._playersLookingForMatches[playersCount - 1].Division)
                {
                    playerindex = i;
                    break;
                }
                if (instance._playersLookingForMatches[i].Division + 1 == instance._playersLookingForMatches[playersCount - 1].Division)
                {
                    playerindex = i;
                    break;
                }
                if (instance._playersLookingForMatches[i].Division - 1 == instance._playersLookingForMatches[playersCount - 1].Division)
                {
                    playerindex = i;
                    break;
                }
            }


            if (playerindex == -1)
            {
                StartTimer();
            }
            else if(playerindex >= 0) 
            {
                StopTimer(playerindex);

                List<PlayerData> players = new List<PlayerData> { instance._playersLookingForMatches[playerindex], instance._playersLookingForMatches[playersCount - 1] };

                _matches.Add(new Match(players, _gameLogger));

                players.ForEach(StartMatch);
            }

            yield return new WaitForSeconds(1f);
        }

        private void StartMatch(PlayerData player)
        {
            player.Connection.Send(new RequestMatchDto());

            RemovePlayerLookingMatch(player);
        }

        private void OnDestroy()
        {
            _timer.Dispose();
        }

        public void StartBotTurn(Match match) 
        {
            StartCoroutine(match.BotTurn());
        }
    }
}