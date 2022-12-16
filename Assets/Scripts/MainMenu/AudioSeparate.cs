using UnityEngine;

namespace MainMenu
{
    public class AudioSeparate : MonoBehaviour
    {
        public AudioSource tavernInsideAudioSource;
        public AudioSource mainMenuAudioSource;

        public GameObject tavernInsideWindow;

        private void OnEnable()
        {
            mainMenuAudioSource.Stop();
            tavernInsideAudioSource.Play();
        }

        private void OnDisable()
        {
            tavernInsideAudioSource.Stop();
            mainMenuAudioSource.Play();
        }
    }
}
