using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using Core.Utils;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Custom/Create new Copping The Tech Effect", order = 52)]
    public class CoppingTheTechEffect : Effect
    {
        public string nameResource;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            BattleResource usedPlayerBattleResource = usedPlayer.Castle.GetResource(nameResource);
            BattleResource enemyPlayerBattleResource = enemyPlayer.Castle.GetResource(nameResource);

            if (usedPlayerBattleResource.Income < enemyPlayerBattleResource.Income)
                enemyPlayerBattleResource.AddIncome(usedPlayerBattleResource.Income - enemyPlayerBattleResource.Income);
        }

        public override string ToString()
        {
            return
                $"If {GetPrettyResourceName()} < enemy {GetPrettyResourceName()}, {GetPrettyResourceName()} = enemy {GetPrettyResourceName()}";
        }
        
        private string GetPrettyResourceName() => ResourcesNamePrettier.GetIncomePrettyName(nameResource);

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            // BattleResource myPlayerResource = BattleClientManager.GetMyData().Castle.GetResource(nameResource);
            // BattleResource enemyPlayerResource = BattleClientManager.GetEnemyData().Castle.GetResource(nameResource);

            // if (isSender)
            // {
            //     if (myPlayerResource.Income < enemyPlayerResource.Income)
            //         BattleUI.AddMyResourceIncome(nameResource, enemyPlayerResource.Income - myPlayerResource.Income);
            // }
            // else
            // {
            //     if (enemyPlayerResource.Income < myPlayerResource.Income)
            //         BattleUI.AddEnemyResourceIncome(nameResource, myPlayerResource.Income - enemyPlayerResource.Income);
            // }

            yield return null;
        }
    }
}