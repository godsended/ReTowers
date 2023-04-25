using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Custom/Create new Parity Effect", order = 52)]
    public class ParityEffect : Effect
    {
        public string nameResource;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            BattleResource usedPlayerBattleResource = usedPlayer.Castle.GetResource(nameResource);
            BattleResource enemyPlayerBattleResource = enemyPlayer.Castle.GetResource(nameResource);

            if (usedPlayerBattleResource.Income > enemyPlayerBattleResource.Income) 
                enemyPlayerBattleResource.AddIncome(usedPlayerBattleResource.Income - enemyPlayerBattleResource.Income);
            else
                usedPlayerBattleResource.AddIncome(enemyPlayerBattleResource.Income - usedPlayerBattleResource.Income);
        }

        public override string ToString()
        {
            return "All players' magic equals the highest player's magic";
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            // BattleResource myPlayerResource = BattleClientManager.GetMyData().Castle.GetResource(nameResource);
            // BattleResource enemyPlayerResource = BattleClientManager.GetEnemyData().Castle.GetResource(nameResource);
            //
            // if (myPlayerResource.Income > enemyPlayerResource.Income)
            //     BattleUI.AddEnemyResourceIncome(nameResource, myPlayerResource.Income - enemyPlayerResource.Income);
            // else
            //     BattleUI.AddMyResourceIncome(nameResource, enemyPlayerResource.Income - myPlayerResource.Income);

            yield return null;
        }
    }
}