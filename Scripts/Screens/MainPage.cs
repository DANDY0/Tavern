using GrandDevs.Tavern.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class MainPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private ILocalizationManager _localizationManager;
        private IGameplayManager _gameplayManager;
        private IDataManager _dataManager;
        private IAppStateManager _appStateManager;
        private IScenesManager _scenesManager;
        private IUserProfileManager _userProfileManager;
        private ModelInputController _modelInputController;

        private Button _playGameButton;
        private Button _playGameWithBotsButton;
        private MenuProfilePanel MenuProfilePanel;
        private MenuCenterPanels MenuCenterPanels;

        private bool IsPanelsInited;
        private bool IsInventoryUpdated;
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _appStateManager = GameClient.Get<IAppStateManager>();
            _scenesManager = GameClient.Get<IScenesManager>();
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _modelInputController = _gameplayManager.GetController<ModelInputController>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/MainPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            UpdateLocalization();
            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            InitPanels();
            
            _selfPage.SetActive(false);
        }
        

        private void InitPanels()
        {
            var contentParent = _selfPage.transform.Find("Content");
            MenuProfilePanel = new MenuProfilePanel(contentParent);
            MenuCenterPanels = new MenuCenterPanels(contentParent);
        }

        private void DisposePanels()
        {
            MenuProfilePanel?.Dispose();
            MenuProfilePanel = null;
            MenuCenterPanels?.Dispose();
            MenuCenterPanels = null;
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
            _modelInputController.SetCurrentModelMenu(_dataManager.CachedUserLocalData.lastChosenCharacterMenu);
        }

		public void Update()
        {
        }

        public void Dispose()
        {
            _localizationManager.LanguageWasChangedEvent -= LanguageWasChangedEventHandler;
            DisposePanels();
        }

        private void UpdateLocalization()
        {
            // _playGameButton.transform.Find("Text_Play").GetComponent<TextMeshProUGUI>().text = _localizationManager.GetUITranslation("BUTTON_PLAY");
        }

        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

 
    }
}