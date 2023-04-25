using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using UnityEngine;
using Effects;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Heal Tower Effect", order = 51)]
    public class HealTowerEffect : Effect
    {
        [Min(0)]
        public int heal;
        public bool isSelfHeal;
        public GameObject EffectAnimation;
        
        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            CastleEntity castle = isSelfHeal ? usedPlayer.Castle : enemyPlayer.Castle;

            castle.Tower.Heal(heal);
        }
        
        public override string ToString()
        {
            return $"+{heal}{(!isSelfHeal ? " to enemy" : "")} tower";
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            if (isSelfHeal)
            {
                if (isSender)
                {
                    //BattleUI.HealMyTower(heal);
                    EffectSpawner.SpawnEffect(
                            EffectSpawner.instance.effectHealFlying,
                            GameObject.Find("SpawnAnimationPlaceHolder").transform.position,
                            BattleUI.Instance.myTower.transform.position);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.HealEnemyTower(heal);
                    EffectSpawner.SpawnEffect(
                            EffectSpawner.instance.effectHealFlying,
                            GameObject.Find("SpawnAnimationPlaceHolder").transform.position,
                            BattleUI.Instance.enemyTower.transform.position);

                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                if (isSender)
                {                 
                    //BattleUI.HealEnemyTower(heal);
                    EffectSpawner.SpawnEffect(
                            EffectSpawner.instance.effectHealFlying,
                            GameObject.Find("SpawnAnimationPlaceHolder").transform.position,
                            BattleUI.Instance.enemyTower.transform.position);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.HealMyTower(heal);
                    EffectSpawner.SpawnEffect(
                            EffectSpawner.instance.effectHealFlying,
                            GameObject.Find("SpawnAnimationPlaceHolder").transform.position,
                            BattleUI.Instance.myTower.transform.position);

                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }
}