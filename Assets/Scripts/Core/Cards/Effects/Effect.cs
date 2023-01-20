using Core.Server;
using System.Collections;
using Core.Match;
using UnityEngine;

namespace Core.Cards.Effects
{
    public abstract class Effect : ScriptableObject
    {
        public abstract void Execute(MatchPlayer usedPlayer, MatchPlayer enemyPlayer);
        public abstract IEnumerator Animation(CardObject cardObject, bool isSender);
    }
}