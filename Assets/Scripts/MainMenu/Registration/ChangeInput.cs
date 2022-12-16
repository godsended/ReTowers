using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MainMenu.Registration
{
    public class ChangeInput : MonoBehaviour
    {
        private EventSystem system;
        public Selectable firstInput;
        public Button submitButton;

        private void Start()
        {
            system = EventSystem.current;
            firstInput.Select();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                var next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                
                if (next != null)
                    next.Select();
            }
            
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                var next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                
                if (next != null)
                    next.Select();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                submitButton.onClick.Invoke();
            }
        }
    }
}

