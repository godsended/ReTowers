using System.Collections;
using UnityEngine;

namespace MainMenu
{
    public class TavernEntrance : MonoBehaviour
    {
        public GameObject openedTavernDoor;
        public GameObject tavernInside;
        public GameObject exitButton;

        public int timeForOpenedDoor = 1;

        private void OnMouseDown()
        {
            ShowOpenedDoor();
        }

        private void OnMouseEnter()
        {
            openedTavernDoor.SetActive(true);
        }

        private void OnMouseExit()
        {
            openedTavernDoor.SetActive(false);
        }

        public void ShowOpenedDoor()
        {
            StartCoroutine(GetInsideTavern());
        }

        private IEnumerator GetInsideTavern()
        {
            openedTavernDoor.SetActive(true);

            yield return new WaitForSeconds(timeForOpenedDoor);

            openedTavernDoor.SetActive(false);

            tavernInside.SetActive(true);
        }

        public void TerminateTavernInside()
        {
            tavernInside.SetActive(false);
        }
    }
}
