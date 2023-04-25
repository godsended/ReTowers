using Core.Castle;
using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using Core.Utils;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Effect Remove BattleResource", order = 51)]
    public class RemoveResourceEffect : Effect
    {
        public string nameResource;
        [Min(0)]
        public int value;
        public bool isSelfRemove;
        public GameObject EffectAnimation;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            CastleEntity castle = isSelfRemove ? usedPlayer.Castle : enemyPlayer.Castle;

            castle.GetResource(nameResource).RemoveResource(value);
        }
        
        public override string ToString()
        {
            return $"-{value}{(!isSelfRemove ? " enemy" : "")} {GetPrettyResourceName()}";
        }
        
        private string GetPrettyResourceName() => ResourcesNamePrettier.GetResourcePrettyName(nameResource);

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            if (isSelfRemove)
            {
                if (isSender)
                {
                    //BattleUI.RemoveMyResourceValue(nameResource, value);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.RemoveEnemyResourceValue(nameResource, value);

                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                if (isSender)
                {
                    //BattleUI.RemoveEnemyResourceValue(nameResource, value);

                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    //BattleUI.RemoveMyResourceValue(nameResource, value);

                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
    }
}
