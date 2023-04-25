using UnityEngine;

namespace Core.Cards
{
    public interface ICardConfigurableObject
    {
        public void SetBackgroundImageSprite(Sprite sprite);

        public void SetForegroundImageSprite(Sprite sprite);

        public void SetTitle(string text);

        public void SetDescription(string description);

        public void SetCostText(string text);
    }
}