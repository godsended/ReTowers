using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Utils
{
    public class GameScenesManager : MonoBehaviour
    {
        private string scene = "Menu";

        private static GameScenesManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            StartCoroutine(LoadSceneAsync("Menu"));
        }

        public static void LoadBattleScene(Action onLoad = null)
        {
            instance.PrepareAndLoadSceneAsync("Battle", onLoad);
        }

        public static void LoadMenuSceneFromBattleScene()
        {
            instance.PrepareAndLoadSceneAsync("Menu");
        }

        public static void ReloadApp()
        {
            if (instance == null)
            {
                SceneManager.LoadScene("LoadingScene");
                return;
            }
            
            if(SceneManager.GetActiveScene().name == "Battle")
                LoadMenuSceneFromBattleScene();
            else if(SceneManager.GetActiveScene().name == "LoadingScene")
                instance.StartCoroutine(instance.LoadSceneAsync("Menu"));
            else
                LoadMenuSceneFromBattleScene();
        }
        
        private void PrepareAndLoadSceneAsync(string scene, Action onLoad = null)
        {
            if (this.scene == scene)
                return;
            this.scene = scene;
            var operation = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Single);
            operation.completed += asyncOperation => StartCoroutine(LoadSceneAsync(scene, onLoad));
        }

        private IEnumerator LoadSceneAsync(string scene, Action onLoad = null)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            asyncLoad.allowSceneActivation = false;
            if(onLoad != null)
                asyncLoad.completed += _ => onLoad();
            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
        }
    }
}