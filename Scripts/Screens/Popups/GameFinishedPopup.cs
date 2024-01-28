using GrandDevs.Tavern.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class GameFinishedPopup: IUIPopup
    {
        private GameObject _selfPopup;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;
        private IGameplayManager _gameplayManager;
        private Button _goMenuButton;

        public GameObject Self => _selfPopup;
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            
            _selfPopup = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/GameFinishedPopup"));
            _selfPopup.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPopup.name = GetType().Name;
            
            _goMenuButton = _selfPopup.transform.Find("Content/Button_GoMenu").GetComponent<Button>();
            _goMenuButton.onClick.AddListener(GoMenuEventHandler);
            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;
            Hide();
        }

        private void GoMenuEventHandler()
        {
            _gameplayManager.GetController<GameEventController>().LeaveGameplay();
            Hide();
        }

        public void Hide()
        {
            _uiManager.FadeScreen(_selfPopup, false, () =>
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
            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEventHandler;
        }

        public void SetMainPriority()
        {
        }
        
        private void SceneForAppStateWasLoadedEventHandler(Enumerators.AppState appState)
        {
            if(appState == Enumerators.AppState.Game)
                Hide();
        }
    }
}