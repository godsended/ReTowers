using UnityEngine;

namespace MainMenu
{
    public class TavernHighlight : MonoBehaviour
    {
        public GameObject tavernOutside;

        private SpriteRenderer tavernColor;

        public Color highlightColor;

        private void Start()
        {
            tavernColor = tavernOutside.GetComponent<SpriteRenderer>();
        }

        private void OnMouseEnter()
        {
            tavernColor.color = highlightColor;
        }

        private void OnMouseExit()
        {
            tavernColor.color = Color.white;
        }
    }
}
