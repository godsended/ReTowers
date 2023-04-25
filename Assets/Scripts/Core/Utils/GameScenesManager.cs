using UnityEngine.SceneManagement;

namespace Core.Utils
{
    public static class GameScenesManager
    {
        private static string scene = "Menu";
        public static void LoadBattleScene()
        {
            if (scene == "Battle")
                return;
            scene = "Battle";
            SceneManager.LoadScene("Battle", LoadSceneMode.Single);
        }

        public static void LoadMenuSceneFromBattleScene()
        {
            if (scene == "Menu")
                return;
            scene = "Menu";
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }
}