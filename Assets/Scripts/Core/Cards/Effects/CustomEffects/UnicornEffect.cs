using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using System.Collections.Generic;
using Core.Match;
using Core.Utils;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Custom/Create new Unicorn Effect", order = 52)]
    public class UnicornEffect : Effect
    {
        public string nameResource;

        [Header("Effect if condition true")]
        public List<Effect> trueEffects;
        [Header("Effect if condition false")]
        public List<Effect> falseEffects;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            BattleResource usedPlayerBattleResource = usedPlayer.Castle.GetResource(nameResource);
            BattleResource enemyPlayerBattleResource = enemyPlayer.Castle.GetResource(nameResource);

            if (usedPlayerBattleResource.Income > enemyPlayerBattleResource.Income)
                trueEffects.ForEach(e => e.Execute(usedPlayer, enemyPlayer));
            else
                falseEffects.ForEach(e => e.Execute(usedPlayer, enemyPlayer));
        }
        
        public override string ToString()
        {
            var trueEf = "";
            var elseEf = "";
            trueEffects.ForEach(e => trueEf += e + "\n");
            falseEffects.ForEach(e => elseEf += e + "\n");
            return $"If {GetPrettyResourceName()} > enemy {GetPrettyResourceName()}, {trueEf}Else {elseEf}";
        }
        
        private string GetPrettyResourceName() => ResourcesNamePrettier.GetIncomePrettyName(nameResource);

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            BattleResource myPlayerBattleResource = BattleClientManager.GetMyData().Castle.GetResource(nameResource);
            BattleResource enemyPlayerBattleResource = BattleClientManager.GetEnemyData().Castle.GetResource(nameResource);

            if (isSender)
            {
                if (myPlayerBattleResource.Income > enemyPlayerBattleResource.Income)
                {
                    foreach (Effect effect in trueEffects)
                        yield return cardObject.StartCoroutine(effect.Animation(cardObject, isSender));
                }
                else
                {
                    foreach (Effect effect in falseEffects)
                        yield return cardObject.StartCoroutine(effect.Animation(cardObject, isSender));
                }
            }
            else
            {
                if (myPlayerBattleResource.Income < enemyPlayerBattleResource.Income)
                {
                    foreach (Effect effect in trueEffects)
                        yield return cardObject.StartCoroutine(effect.Animation(cardObject, isSender));
                }
                else
                {
                    foreach (Effect effect in falseEffects)
                        yield return cardObject.StartCoroutine(effect.Animation(cardObject, isSender));
                }
            }

            yield return null;
        }
    }
}