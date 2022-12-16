using UnityEngine;

namespace MainMenu.Libary {
    public class ResoursesFiltr : MonoBehaviour
    {
        [SerializeField] ShowLibary showLibary;
        [SerializeField] GameObject[] filtr;
        private bool isOpen;

        public void ShowOrCloseFiltrMenu() 
        {
            if (isOpen) {
                foreach (var item in filtr)
                {
                    item.SetActive(false);
                }
            }
            else {
                foreach (var item in filtr)
                {
                    item.SetActive(true);
                }
            }
            isOpen = !isOpen;
        }

        public void SetFiltr(string typeString)
        {
            ResoursType type = new ResoursType();
            switch (typeString) 
            {
                case "None":
                    type = ResoursType.None;
                    break;
                case "Resource_1":
                    type = ResoursType.Resource_1;
                    break;
                case "Resource_2":
                    type = ResoursType.Resource_2;
                    break;
                case "Resource_3":
                    type = ResoursType.Resource_3;
                    break;
            }
            showLibary.SetResoursesType(type);
        }
    }

    public enum ResoursType
    {
        None,
        Resource_1,
        Resource_2,
        Resource_3
    }
}
