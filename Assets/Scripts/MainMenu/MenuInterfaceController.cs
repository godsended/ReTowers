using UnityEngine;
using UnityEngine.UI;

namespace MainMenu {
    public class MenuInterfaceController : MonoBehaviour
    {
        private static MenuInterfaceController instance;

        [SerializeField] private Button libaryButton;
        [SerializeField] private Button searchMatchButton;
        [SerializeField] private Button adventureButton;

        private void Start()
        {
            instance = this;
        }

        public static void ChangeStateInteface(bool state) 
        {
            instance.libaryButton.interactable = state;
            instance.searchMatchButton.interactable = state;
            instance.adventureButton.interactable = state;
        }
    }
}
