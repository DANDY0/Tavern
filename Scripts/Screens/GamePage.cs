using System;
using GrandDevs.Tavern.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class GamePage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IDataManager _dataManager;
        private IAppStateManager _appStateManager;
        private IInputManager _inputManager;
        private INetworkManager _networkManager;
        private IGameplayManager _gameplayManager;

        private RoundInfoController _roundInfoController;
        private RoundInfoPanel _roundInfoPanel;
        private PlayerProfilePanel _playerProfilePanel;
        private TeamsPanel _teamsPanel;
        private TurnPlayersPanel _turnPlayersPanel;
        private PlayerStatsPanel _playerStatsPanel;

        private TextMeshProUGUI _roundStateText;
        private TextMeshProUGUI _roundCounterText;
        private TextMeshProUGUI _roundTimeText;

        private Transform _contentParent;
        private Transform _controllersParent;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _inputManager = GameClient.Get<IInputManager>();
            _networkManager = GameClient.Get<INetworkManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/GamePage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPage.name = GetType().Name;
            
            _controllersParent = _selfPage.transform.Find("Controllers");
            _contentParent = _selfPage.transform.Find("Content");
            
            // _roundCounterText = _selfPage.transform.Find("Content/RoundInfo/Text_RoundCounter").GetComponent<TextMeshProUGUI>();
            // _roundStateText = _selfPage.transform.Find("Content/RoundInfo/Text_RoundState").GetComponent<TextMeshProUGUI>();
            // _roundTimeText = _selfPage.transform.Find("Content/RoundInfo/Text_RoundTime").GetComponent<TextMeshProUGUI>();
            
            UpdateLocalization();
            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            
            var roundInfoController = _gameplayManager.GetController<RoundInfoController>();
            roundInfoController.AssignUIElements(_roundCounterText, _roundStateText, _roundTimeText);
        
            InitPanels();
            
            _selfPage.SetActive(false);
        }

        public void Hide()
        {
            _uiManager.FadeScreen(_selfPage, false, () =>
            {
                _selfPage.SetActive(false);
            });
        }

        public void Show()
        {
            _selfPage.SetActive(true);

            _uiManager.FadeScreen(_selfPage, true);
        }

        public void Update()
        {
            if(_appStateManager.AppState == Enumerators.AppState.Game)
            {
                if(Input.GetKeyDown(KeyCode.Escape))
                {
                    _gameplayManager.GetController<GameEventController>().LeaveGameplay();
                }
            }
        }

        public void Dispose()
        {
            DisposePanels();
            _localizationManager.LanguageWasChangedEvent -= LanguageWasChangedEventHandler;
        }

        public Transform GetControllsParent()
        {
            return _controllersParent;
        }

        private void InitPanels()
        {
            _roundInfoPanel = new RoundInfoPanel(_contentParent);
            _playerProfilePanel = new PlayerProfilePanel(_contentParent);
            _teamsPanel = new TeamsPanel(_contentParent);
            _turnPlayersPanel = new TurnPlayersPanel(_contentParent);
            _playerStatsPanel = new PlayerStatsPanel(_contentParent);
        }

        private void DisposePanels()
        {
            _roundInfoPanel?.Dispose();
            _roundInfoPanel = null;

            _playerProfilePanel?.Dispose();
            _playerProfilePanel = null;

            _teamsPanel?.Dispose();
            _teamsPanel = null;

            _turnPlayersPanel?.Dispose();
            _turnPlayersPanel = null;
            
            _playerStatsPanel?.Dispose();
            _playerStatsPanel = null;
        }

        private void UpdateLocalization()
        {

        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }
    }
}