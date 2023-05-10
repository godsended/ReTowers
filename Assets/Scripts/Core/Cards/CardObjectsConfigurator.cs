using System.Text;
using UnityEngine;

namespace Core.Cards
{
    public class CardObjectsConfigurator : MonoBehaviour
    {
        private static CardObjectsConfigurator instance;

        [SerializeField] private Sprite[] blueBackgroundSprites;
        
        [SerializeField] private Sprite[] redBackgroundSprites;
        
        [SerializeField] private Sprite[] greenBackgroundSprites;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static void Configure(ICardConfigurableObject card, CardData data)
        {
            switch (data.Cost[0].Name)
            {
                case "Resource_1":
                    card.SetBackgroundImageSprite(instance.greenBackgroundSprites[data.Rang]);
                    break;
                case "Resource_2":
                    card.SetBackgroundImageSprite(instance.blueBackgroundSprites[data.Rang]);
                    break;
                case "Resource_3":
                    card.SetBackgroundImageSprite(instance.redBackgroundSprites[data.Rang]);
                    break;
            }
            card.SetCostText(data.Cost[0].Value.ToString());
            card.SetForegroundImageSprite(data.CardImage);
            card.SetTitle(data.Name);

            if (!string.IsNullOrEmpty(data.Description))
            {
                card.SetDescription(data.Description);
                return;
            }

            StringBuilder description = new StringBuilder();
            data.Effects.ForEach(e => description.Append($"{e}, "));
            description.Remove(description.Length - 2, 2);
            card.SetDescription(description.ToString());
        }
    }
}