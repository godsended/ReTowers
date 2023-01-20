using Core.Client;
using Core.Server;
using System.Collections;
using System.Collections.Generic;
using Core.Match;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Custom/Create new Foundations Effect", order = 52)]
    public class FoundationsEffect : Effect
    {
        [Header("Effect if condition true")]
        public List<Effect> trueEffects;
        [Header("Effect if condition false")]
        public List<Effect> falseEffects;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            if (usedPlayer.Castle.Wall.Health == 0)
                trueEffects.ForEach(e => e.Execute(usedPlayer, enemyPlayer));
            else
                falseEffects.ForEach(e => e.Execute(usedPlayer, enemyPlayer));
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            if (isSender)
            {
                if (BattleClientManager.GetMyData().Castle.Wall.Health == 0)
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
                if (BattleClientManager.GetEnemyData().Castle.Wall.Health == 0)
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
        }
    }
}