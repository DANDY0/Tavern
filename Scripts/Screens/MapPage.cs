using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Panel.Inventory;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class MapPage : IUIElement
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
        
        private Transform _contentParent;
        private MapPanel _mapPanel;

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

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/MapPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            _selfPage.name = GetType().Name;
            
            _contentParent = _selfPage.transform.Find("Content");

            UpdateLocalization();
            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            
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
            if(_mapPanel!=null)
                _mapPanel.Update();
        }

        public void Dispose()
        {
            DisposePanels();
            _localizationManager.LanguageWasChangedEvent -= LanguageWasChangedEventHandler;
        }

        private void InitPanels()
        {
            _mapPanel = new MapPanel(_contentParent);
        }

        private void DisposePanels()
        {
            _mapPanel.Dispose();
            _mapPanel = null;
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