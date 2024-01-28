using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Panel.Inventory;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class InventoryPage : IUIElement
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
        private ModelInputController _modelInputController;

        private TabsPanel _tabsPanel;
        private MainInventoryPanel _mainInventoryPanel;
        private CharacterInventoryPanel _characterInventoryPanel;

        private Transform _contentParent;

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
            _modelInputController = _gameplayManager.GetController<ModelInputController>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/InventoryPage"));
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
            _mainInventoryPanel.Show(false);
            _characterInventoryPanel.Show(false);
            _modelInputController.SetCanHandleInput(true);
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            
            _uiManager.FadeScreen(_selfPage, true);
            _characterInventoryPanel.Show(true);
            _modelInputController.SetCanHandleInput(false);
            _tabsPanel.SetFirstTabOn();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            DisposePanels();
            _localizationManager.LanguageWasChangedEvent -= LanguageWasChangedEventHandler;
        }

        private void InitPanels()
        {
            _mainInventoryPanel = new MainInventoryPanel(_contentParent);
            _characterInventoryPanel = new CharacterInventoryPanel(_contentParent);
            _tabsPanel = new TabsPanel(_contentParent, _mainInventoryPanel, _characterInventoryPanel);
        }

        private void DisposePanels()
        {
            _tabsPanel.Dispose();
            _mainInventoryPanel.Dispose();
            _characterInventoryPanel.Dispose();
            _tabsPanel = null;
            _mainInventoryPanel = null;
            _characterInventoryPanel = null;
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