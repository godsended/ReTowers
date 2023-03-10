using System;
using System.Collections.Generic;
using Core.Cards.Effects;
using Core.Castle;
using UnityEngine;

namespace Core.Cards
{
    [CreateAssetMenu(fileName = "New CardData", menuName = "Create new CardData", order = 51)]
    public class CardData : ScriptableObject
    {
        [TextArea(minLines: 1, maxLines: 4)]
        [Tooltip("Global id instance")]
        public string Id;
        [Tooltip("Name card")]
        public string Name;
        [Tooltip("Image card")]
        public Sprite CardImage;
        [Tooltip("Cost")]
        public List<Resource> Cost;
        [Tooltip("Effects on card")]
        public List<Effect> Effects;
        [Tooltip("Effect on card: Play Again")]     
        public bool SaveTurn;
        [Tooltip("Effect on card: This card can't be discarded")]
        public bool NonDiscard;
        [Tooltip("Description card")]
        public string Description;
        [Tooltip("Card in deck")]
        public bool InDeck;
        [Tooltip("Count card")]
        public int Count;
        [Tooltip("Type of Card")]
        public string Type;
        [Tooltip("Rang Card")]
        public int Rang;
        [Tooltip("Is available for player")] 
        public bool IsCollectible = true;

        public CardData()
        {
            Id = Guid.NewGuid().ToString();
        }

        public void Regenerate() 
        {
            Id = Guid.NewGuid().ToString();
        }

        public void UpLevel() 
        {
            foreach(var item in LibraryCards.GetPlayerCards()) 
            {
                if(item.Type == Type && item.Rang - 1 == Rang) 
                {
                    item.Count++;
                    return;
                }
            }
        }
    }
}