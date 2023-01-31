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
using Core.Cards;
using Core.Map;
using Core.Match;
using Core.Match.Server;
using Core.Utils.NameGenerator;

namespace Core.Server
{
    /// <summary>
    /// Match controller
    /// Responsible for the logic of finding a match among players
    /// </summary>
    [DisallowMultipleComponent]
    public class MatchServerController : MonoBehaviour
    {
        private static INameGenerator nameGenerator = new MarkNameGenerator();
        
        public int TimeToStartBot = 45;
        
        public static MatchServerController instance;

        /// <summary>
        /// Number of players in a match
        /// Default: 2
        /// </summary>
        public int MatchSize { get; set; } = 2;

        private List<PlayerData> _playersLookingForMatches;
        private List<MatchServer> _matches { get; set; }
        private IGameLogger _gameLogger;
        private Timer _timer;

        [SerializeField]private List<int> _times;
        
        /// <summary>
        /// Add a player who is looking for a match
        /// </summary>
        /// <param name="player">Looking player</param>
        public static void RemoveMatch(Guid matchId)
        {
            MatchServer match = instance._matches.FirstOrDefault(m => m.MatchId == matchId);

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
                    .FirstOrDefault(m => m.MatchDetails.Players
                        .FirstOrDefault(p => p.PlayFabId == player.Id.ToString()) != null);

                if (match != null)
                    match.MatchDetails.Players.Remove(match.MatchDetails.Players.FirstOrDefault(p 
                        => p.PlayFabId == player.PlayFabId));

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
            _matches = new List<MatchServer>();
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
                    .FirstOrDefault(m => m.MatchDetails.Players
                        .FirstOrDefault(p => p.Connection == serverConnect) != null);

                //Здесь будет вызов плейфаба
                if (match != null)
                {
                    foreach (MatchPlayer player in match.MatchDetails.Players)
                    {
                        player?.Connection?.Send(new RequestMatchDto
                        {
                            RequestType = MatchRequestType.WinMatch
                        });
                    }

                    match.Stop();
                }
            });

            NetworkServer.RegisterHandler<RequestBattleInfo>((connection, requestBattleDto) =>
            {
                var match = _matches
                    .FirstOrDefault(m => m.MatchDetails.Players
                        .FirstOrDefault(p => p.PlayFabId == requestBattleDto.AccountId.ToString()) != null);

                if (match != null)
                {
                    var yourData = match.MatchDetails.Players.FirstOrDefault(p => p.PlayFabId 
                        == requestBattleDto.AccountId.ToString());
                    var enemyData = match.MatchDetails.Players.FirstOrDefault(p => p.PlayFabId 
                        != requestBattleDto.AccountId.ToString());

                    DivisionCastleCreator castleCreator = new DivisionCastleCreator(match.MatchDetails.Division);
                    MatchPlayerDataInitializer<DivisionCastleCreator> playerDataInitializer = new(castleCreator);
                    playerDataInitializer.Initialize(yourData);
                    PlayerData enemyPlayerData = MainServer.GetPlayerData(Guid.Parse(enemyData.PlayFabId));
                    if (enemyData != null)
                    {
                        playerDataInitializer.Initialize(enemyData);
                        Debug.Log($"Sending ReqBattleInfo with division {match.MatchDetails.Division}");
                        connection.Send(new RequestBattleInfo
                        {
                            AccountId = requestBattleDto.AccountId,
                            YourName = yourData.Name,
                            EnemyName = enemyData.Name,
                            YourTowerHealth = yourData.Castle.Tower.Health,
                            EnemyTowerHealth = enemyData.Castle.Tower.Health,
                            YourWallHealth = yourData.Castle.Wall.Health,
                            EnemyWallHealth = enemyData.Castle.Wall.Health,
                            IsYourTurn = match.MatchDetails.CurrentPlayer.PlayFabId == requestBattleDto.AccountId.ToString(),
                            Timer = (int)match.MatchDetails.TurnTime,
                            EnemyWinCount = enemyPlayerData.PlayerStatistics.WinCount,
                            StartDamageFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueDamageStart"]),
                            TurnFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]),
                            FatigueLimit = int.Parse(Configurator.data["BattleConfiguration"]["fatigueLimit"]),
                            Division = match.MatchDetails.Division
                        });

                        match.MatchDetails.Players.FirstOrDefault(p => p.PlayFabId == requestBattleDto.AccountId.ToString())!.IsReady = true;
                    }
                    else 
                    {

                        string[] names = File.ReadAllLines(Application.dataPath + "/StreamingAssets" + "/names.txt");

                        PlayerData yourPlayerData = MainServer.GetPlayerData(Guid.Parse(yourData.PlayFabId));
                        connection.Send(new RequestBattleInfo
                        {
                            AccountId = requestBattleDto.AccountId,
                            YourName = yourData.Name,
                            EnemyName = names[UnityEngine.Random.Range(0, names.Length)],
                            YourTowerHealth = yourData.Castle.Tower.Health,
                            EnemyTowerHealth = yourData.Castle.Tower.Health,
                            YourWallHealth = yourData.Castle.Wall.Health,
                            EnemyWallHealth = yourData.Castle.Wall.Health,
                            IsYourTurn = match.MatchDetails.CurrentPlayer.PlayFabId == requestBattleDto.AccountId.ToString(),
                            Timer = (int)match.MatchDetails.TurnTime,
                            EnemyWinCount = UnityEngine.Random.Range(yourPlayerData.PlayerStatistics.WinCount, yourPlayerData.PlayerStatistics.WinCount + 5),
                            StartDamageFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueDamageStart"]),
                            TurnFatigue = int.Parse(Configurator.data["BattleConfiguration"]["fatigueTurnStart"]),
                            FatigueLimit = int.Parse(Configurator.data["BattleConfiguration"]["fatigueLimit"]),
                            Division = match.MatchDetails.Division
                        });
                        match.MatchDetails.Players.FirstOrDefault(p => p.PlayFabId == requestBattleDto.AccountId.ToString())!.IsReady = true;
                    }
                }
                else
                {
                    _gameLogger.Log($"Match, contains player {requestBattleDto.AccountId} not founded!", LogTypeMessage.Low);
                }
            }, false);

            NetworkServer.RegisterHandler<RequestMatchDto>(HandleSearchMatchRequest, false);
        }

        private void LeaveMatch(PlayerData player)
        {
            var match = _matches
                .FirstOrDefault(m => m.MatchDetails.Players
                    .FirstOrDefault(p => p.PlayFabId == player.PlayFabId) != null);

            if (match != null)
            {
                player.Castle.Tower.Damage(player.Castle.Tower.Health);
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
                    StartToBot(null, null, _playersLookingForMatches[i]);
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

        public void StartToBot(LevelInfo levelInfo, MapProgress mapProgress, PlayerData player)
        {
            // if (player == null)
            // {
            //     player = instance._playersLookingForMatches.First();
            // }

            MatchServer.MatchServerCreator creator = new MatchServer.MatchServerCreator();
            MatchServer match = levelInfo == null || mapProgress == null
                ? creator.CreateBotMatchServer(false)
                : creator.CreateBotMatchServer(true);
            match.MatchDetails.LevelInfo = levelInfo;
            match.MatchDetails.MapProgress = mapProgress;
            match.AddPlayer(player.PlayFabId, player.Name, player.Cards, player.Division, player.Connection);
            List<Guid> cards = new List<Guid>();
            player.Cards.CardsIdDeck.ForEach(c => cards.Add(c));
            player.Cards.CardsIdHand.ForEach(c => cards.Add(c));
            PlayerCards botCards = new PlayerCards(cards);
            match.AddBot(botCards, nameGenerator.Generate());
            _matches.Add(match);

            StartMatch(player);
        }
        
        private IEnumerator CheckMatching()
        {
            if (instance._playersLookingForMatches.Count <= 0)
                yield break;

            int playerindex = -1;
            int playersCount = instance._playersLookingForMatches.Count;

            Debug.Log($"Weee {instance._playersLookingForMatches[playersCount - 1].Division}");
            for (int i = 0; i < playersCount - 1; i++)
            {
                Debug.Log($"Hewww {instance._playersLookingForMatches[i].Division}");
                if (Mathf.Abs(instance._playersLookingForMatches[i].Division - instance._playersLookingForMatches[playersCount - 1].Division) < 2)
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

                MatchServer match = new MatchServer.MatchServerCreator().CrateMatchServer();
                foreach (var player in players)
                {
                    player.CurrentMatch = match;
                    match.AddPlayer(player.PlayFabId, player.Name, player.Cards, player.Division, player.Connection);
                    player.Connection.Send(new LoadBattleSceneDto()
                    {
                        MatchId = match.MatchId.ToString(),
                        RequestId = match.MatchId.ToString() + player.PlayFabId
                    });
                }
                _matches.Add(match);
                yield return new WaitForSeconds(1);
                match.Start();

                players.ForEach(StartMatch);
            }
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

        private void HandleSearchMatchRequest(NetworkConnectionToClient connection, RequestMatchDto requestMatchDto)
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
                        ServerPlayfabManager.instance.GetUserData(playerData, true, requestMatchDto.LevelId);
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
        }

        private void RequestEndTurn(PlayerData playerData)
        {
            Debug.Log("Request to force pass the move!");
            playerData.CurrentMatch.PassTheMove(true);
            playerData.CurrentMatch.SendOutMatchDetails();
        }
    }
}