using System.Collections;
using Core.Client;
using Core.Match;
using UnityEngine;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new Boss Ability", order = 51)]
    public class BossAbilityEffect : Effect
    {
        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            BattleUI.Instance.BossView.PlayCastAnimation();
            yield break;
        }
        
        public override string ToString()
        {
            return "Boss ability";
        }
    }
}