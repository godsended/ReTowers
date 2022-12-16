using UnityEngine;

namespace MainMenu.Libary {
    public class RareFiltr : MonoBehaviour
    {
        [SerializeField] ShowLibary showLibary;
        [SerializeField] GameObject[] filtr;
        private bool isOpen;

        public void ShowOrCloseFiltrMenu()
        {
            if (isOpen)
            {
                foreach (var item in filtr)
                {
                    item.SetActive(false);
                }
            }
            else
            {
                foreach (var item in filtr)
                {
                    item.SetActive(true);
                }
            }
            isOpen = !isOpen;
        }

        public void SetFiltr(int lvl)
        {
            showLibary.SetLevelFiltr(lvl);
        }
    }
}
