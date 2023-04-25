using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using Core.Utils;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Effect Remove Income", order = 51)]
    public class RemoveIncomeEffect : Effect
    {
        public string nameResource;
        [Min(0)]
        public int income;
        public bool isSelfRemove;
        public GameObject EffectAnimation;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            CastleEntity castle = isSelfRemove ? usedPlayer.Castle : enemyPlayer.Castle;

            castle.GetResource(nameResource).RemoveIncome(income);
        }
        
        public override string ToString()
        {
            return $"-{income}{(!isSelfRemove ? " enemy" : "")} {GetPrettyResourceName()}";
        }
        
        private string GetPrettyResourceName() => ResourcesNamePrettier.GetIncomePrettyName(nameResource);

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            if (isSelfRemove)
            {
                if (isSender)
                {
                    //BattleUI.RemoveMyResourceIncome(nameResource, income);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.RemoveEnemyResourceIncome(nameResource, income);

                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                if (isSender)
                {
                    //BattleUI.RemoveEnemyResourceIncome(nameResource, income);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.RemoveMyResourceIncome(nameResource, income);

                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }
}
