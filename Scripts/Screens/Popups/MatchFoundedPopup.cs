using System.Collections;
using GrandDevs;
using DG.Tweening;
using GrandDevs.Networking;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using UnityEngine.UI;
using static GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class MatchFoundedPopup: IUIPopup
    {
        private GameObject _selfPopup;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;
        private INetworkManager _networkManager;
        private IGameplayManager _gameplayManager;
        private ConnectionController _connectionController;
        private GameplayController _gameplayController;
        private Button _readyButton;

        private Sequence _timerSequence;

        public GameObject Self => _selfPopup;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _connectionController = _gameplayManager.GetController<ConnectionController>();
            _gameplayController = _gameplayManager.GetController<GameplayController>();

            _selfPopup = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/MatchFoundedPopup"));
            _selfPopup.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPopup.name = GetType().Name;
            
            _readyButton = _selfPopup.transform.Find("Container/Button_Ready").GetComponent<Button>();

            _readyButton.onClick.AddListener(SetPlayerReady);

            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent += SceneForAppStateWasLoadedEventHandler;

            Hide();
        }

        private void SendReadyToNetwork()
        {
            var gameplayController = _gameplayManager.GetController<GameplayController>();
            var localPlayer = gameplayController.GetLocalPlayer();
            var playerData = localPlayer.GetPlayerData();
            var playerID = playerData.PlayerID;
            _networkManager.SendGameEvent(new ClientGameplayEvent<PlayerReadyData>()
                {
                    data = new PlayerReadyData()
                    {
                        id = playerID
                    },
                    type = Enumerators.GameplayEventType.PlayerReady
                });
        }

        public void Hide()
        {
            StopTimer();            
            
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
            SetButtonInteractable(true);
            
            _uiManager.FadeScreen(_selfPopup, true);
            _timerSequence = InternalTools.DoActionDelayed(() =>
            {
                _connectionController.Disconnect();
                _gameplayController.ResetPlayersList();
                StopTimer();
                Hide();
            }, 10);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            _readyButton.onClick.RemoveListener(SetPlayerReady);
            GameClient.Get<IScenesManager>().SceneForAppStateWasLoadedEvent -= SceneForAppStateWasLoadedEventHandler;
        }

        public void SetMainPriority()
        {
        }

        private void SceneForAppStateWasLoadedEventHandler(AppState appState)
        {
            // if(appState == AppState.Game)
                // Hide();
        }

        private void SetPlayerReady()
        {
            StopTimer();
            SendReadyToNetwork();
            SetButtonInteractable(false);
        }

        private void SetButtonInteractable(bool state)
        {
            _readyButton.interactable = state;
        }

        private void StopTimer()
        {
            if (_timerSequence != null)
                _timerSequence.Kill();
        }
    }
}