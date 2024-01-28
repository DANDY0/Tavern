using DG.Tweening;
using System;
using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class LoadingPopup : IUIPopup
    {
        private GameObject _selfPopup;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;
        private IScenesManager _scenesManager;
        private IGameplayManager _gameplayManager;
        private GameplayController _gameplayController;

        public GameObject Self => _selfPopup;
        private CanvasGroup _canvasGroup;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();
            _gameplayController.OnPlayersSpawned += PlayersSpawnedEventHandler;
            _scenesManager = GameClient.Get<IScenesManager>();
            _scenesManager.SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEvent;
            _selfPopup = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoadingPopup"));
            _selfPopup.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPopup.name = GetType().Name;
            _canvasGroup = _selfPopup.GetComponent<CanvasGroup>();

            Hide();
        }

        private void SceneForAppStateWasLoadedEvent(Enumerators.AppState state)
        {
            if (state == Enumerators.AppState.Main)
                if (_uiManager != null)
                    _uiManager.HidePopup<LoadingPopup>();
        }

        private void PlayersSpawnedEventHandler() => Hide();

        public void Hide()
        {
            _uiManager.FadeScreen(_selfPopup, true, () =>
            {
                _selfPopup.SetActive(false);
            });
        }
        
        public void Show(object data)
        {
            Show();           
        }

        public void Show()
        {
            _selfPopup.SetActive(true);

            _uiManager.FadeScreen(_selfPopup, true);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _gameplayController.OnPlayersSpawned -= PlayersSpawnedEventHandler;
            _scenesManager.SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEvent;
        }

        public void SetMainPriority()
        {
        }
    }
}