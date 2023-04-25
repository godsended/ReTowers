using Core.Castle;
//using Core.Client;
using Core.Server;
using System.Collections;
using Core.Match;
using Core.Utils;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Effect Add Income", order = 51)]
    public class AddIncomeEffect : Effect
    {
        public string nameResource;
        [Min(0)]
        public int income;
        public bool isSelfAdd;
        public GameObject EffectAnimation;

        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            CastleEntity castle = isSelfAdd ? usedPlayer.Castle : enemyPlayer.Castle;

            castle.GetResource(nameResource).AddIncome(income);
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            yield break;
        }

        public override string ToString()
        {
            return $"+{income} {GetPrettyResourceName()}" + (!isSelfAdd ? " to enemy" : "");
        }

        private string GetPrettyResourceName() => ResourcesNamePrettier.GetIncomePrettyName(nameResource);
    }
}
