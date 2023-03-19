using System.Collections;
using Core.Client;
using Core.Match;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Cards.Effects
{
    [CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Create new MyWallHpColorizer", order = 51)]
    public class MyWallHpColorizer : Effect
    {
        [FormerlySerializedAs("Color")] public Color color = Color.white;
        public override void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer)
        {
            
        }

        public override IEnumerator Animation(CardObject cardObject, bool isSender)
        {
            BattleUI.Instance.textHealthMyWall.color = color;
            yield break;
        }
    }
}