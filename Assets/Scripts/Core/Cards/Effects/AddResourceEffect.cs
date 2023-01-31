using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Effect Add Resource", order = 51)]
    public class AddResourceEffect : Effect
    {
        public string nameResource;
        [Min(0)]
        public int value;
        public bool isSelfAdd;
        public GameObject EffectAnimation;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            CastleEntity castle = isSelfAdd ? usedPlayer.Castle : enemyPlayer.Castle;

            castle.GetResource(nameResource).AddResource(value);
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            yield break;
            // if (isSelfAdd)
            // {
            //     if (isSender)
            //     {
            //         BattleUI.AddMyResourceValue(nameResource, value);
            //
            //         yield return new WaitForSeconds(0.2f);
            //     }
            //     else
            //     {
            //         BattleUI.AddEnemyResourceValue(nameResource, value);
            //
            //         yield return new WaitForSeconds(0.2f);
            //     }
            // }
            // else
            // {
            //     if (isSender)
            //     {
            //         BattleUI.AddEnemyResourceValue(nameResource, value);
            //
            //         yield return new WaitForSeconds(0.2f);
            //     }
            //     else
            //     {
            //         BattleUI.AddMyResourceValue(nameResource, value);
            //
            //         yield return new WaitForSeconds(0.2f);
            //     }
            // }
        }
    }
}
