#if !UNITY_ANDROID

using System;
using System.Linq;
using Core.Castle;
using Core.Economics;
using Core.Map;
using Core.Match.Server;
using UnityEngine;

namespace Core.Match.Modifiers.Providers
{
    public class HardCodedPveModifiersProvider : IModifiersProvider
    {
        public void Provide(MatchServer match, LevelInfo level)
        {
            if (level == null)
            {
                var pvpRewardModificator = new PvpMatchRewardModificator(match);
                return;
            }

            string currencyName = "Pve";
            double reward = 1;
            switch (level.Progress)
            {
                case 0:
                    var players = match.MatchDetails.Players.Where(p => p is not IMatchBot);
                    PveMatchFatigueWhiteListModificator fatigueMod0 = new(match, players);

                    MatchCastleAddition myAddition0 = new()
                    {
                        TowerAddition = 10,
                        WallAddition = 10,
                        ResourcesAddition = new[]
                        {
                            new BattleResource("Resource_1", 0, 2),
                            new BattleResource("Resource_2", 0, 2),
                            new BattleResource("Resource_3", 0, 2),
                        }
                    };
                    PveMatchCastlesModificator castlesMod0 = new(match, myAddition0,
                        MatchCastleAddition.Empty);
                    break;

                case 1:
                    var players1 = match.MatchDetails.Players.Where(p => p is not IMatchBot);
                    PveMatchFatigueWhiteListModificator fatigueMod1 = new(match, players1);

                    MatchCastleAddition myAddition1 = new()
                    {
                        TowerAddition = 7,
                        WallAddition = 7,
                        ResourcesAddition = new[]
                        {
                            new BattleResource("Resource_1", 0, 1),
                            new BattleResource("Resource_2", 0, 1),
                            new BattleResource("Resource_3", 0, 1),
                        }
                    };
                    PveMatchCastlesModificator castlesMod1 = new(match, myAddition1,
                        MatchCastleAddition.Empty);
                    break;

                case 2:
                    MatchCastleAddition myAddition2 = new()
                    {
                        TowerAddition = 7,
                        WallAddition = 7,
                        ResourcesAddition = Array.Empty<BattleResource>()
                    };
                    PveMatchCastlesModificator castlesMod2 = new(match, myAddition2,
                        MatchCastleAddition.Empty);
                    break;

                case 3:
                    break;

                case 4:
                    MatchCastleAddition botAddition4 = new()
                    {
                        TowerAddition = 0,
                        WallAddition = 0,
                        ResourcesAddition = new[]
                        {
                            new BattleResource("Resource_1", 0, 1),
                            new BattleResource("Resource_2", 0, 1),
                            new BattleResource("Resource_3", 0, 1),
                        }
                    };
                    PveMatchCastlesModificator castlesMod4 = new(match, MatchCastleAddition.Empty,
                        botAddition4);
                    reward = 2;
                    break;

                case 5:
                    PveMatchUnlimitedFatigueModificator fatigueMod5 = new(match);

                    MatchCastleAddition botAddition5 = new()
                    {
                        TowerAddition = 0,
                        WallAddition = 0,
                        ResourcesAddition = new[]
                        {
                            new BattleResource("Resource_1", 0, 1),
                            new BattleResource("Resource_2", 0, 1),
                            new BattleResource("Resource_3", 0, 1),
                        }
                    };
                    PveMatchCastlesModificator castlesMod5 = new(match, MatchCastleAddition.Empty,
                        botAddition5);
                    reward = 2;
                    break;

                case 6:
                    var player = match.MatchDetails.Players.First(p => p is not IMatchBot);
                    switch (level.BiomeId)
                    {
                        case 0:
                            CardsDiscardBossModificator cardsDiscardBossModificator = new(match, player, 0.25);
                            currencyName = "Pve0";
                            break;
                        case 1:
                            HealFreezeBossModificator freezeBossModificator = new(match, player, 0.25);
                            currencyName = "Pve1";
                            break;
                        case 2:
                            IncomeBurnBossModificator incomeBurnBossModificator = new(match, player, 0.25);
                            currencyName = "Pve2";
                            break;
                    }
                    break;
            }

            var rewardModificator =
                new PveMatchRewardModificator(match, new Currency(currencyName, reward));
        }
    }
}

#endif