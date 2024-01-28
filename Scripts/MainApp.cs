using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GrandDevs.Tavern
{
    public class MainApp : MonoBehaviour
    {
        public delegate void MainAppDelegate(object param);
        public event MainAppDelegate OnLevelWasLoadedEvent;

        public event Action LateUpdateEvent;
        public event Action FixedUpdateEvent;

        private static MainApp _Instance;
        public static MainApp Instance
        {
            get { return _Instance; }
            private set { _Instance = value; }
        }
        

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (gameObject != null) 
                DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Instance == this)
            {
                GameClient.Instance.InitServices();
                GameClient.Get<IDataManager>().LoadConfigurationData();
                SceneManager.sceneLoaded += SceneLoadedHandler;
            }
        }

        private void Update()
        {
            if (Instance == this)
            {
                GameClient.Instance.Update();
            }
        }

        private void LateUpdate()
        {
            if (Instance == this)
            {
                if (LateUpdateEvent != null)
                    LateUpdateEvent();
            }
        }

        private void FixedUpdate()
        {
            if (Instance == this)
            {
                if (FixedUpdateEvent != null)
                    FixedUpdateEvent();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                GameClient.Instance.Dispose();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                if (!Application.isEditor)
                    GameClient.Get<IGameplayManager>().SetPauseStatusOfGameplay(true);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (!Application.isEditor)
                    GameClient.Get<IGameplayManager>().SetPauseStatusOfGameplay(true);
            }
        }

        private void SceneLoadedHandler(Scene scene, LoadSceneMode loadMode)
        {
            Debug.Log("SCENE LOADED");
            if (Instance == this)
            {
                if (OnLevelWasLoadedEvent != null) 
                    OnLevelWasLoadedEvent(scene.buildIndex);
            }
        }
    }
}