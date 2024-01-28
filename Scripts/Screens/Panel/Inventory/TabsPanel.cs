using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GrandDevs.Tavern.Panel.Inventory
{
    public class TabsPanel
    {
        private readonly IUIManager _uiManager;
        private readonly IGameplayManager _gameplayManager;
        private readonly IUserProfileManager _userProfileManager;
        private readonly GameplayController _gameplayController;

        private readonly MainInventoryPanel _mainInventoryPanel;
        private readonly CharacterInventoryPanel _characterInventoryPanel;
        private GameObject _selfObject;
        private Transform _contentParent;

        private Toggle _mainPanelToggle;
        private Toggle _characterPanelToggle;
        private Button _backButton;

        private readonly string _tabsPanelPath = "Prefabs/UI/InventoryPageUI/TabsPanel/TabsPanel";

        public TabsPanel(Transform contentParent, MainInventoryPanel mainInventoryPanel, CharacterInventoryPanel characterInventoryPanel)
        {
            _contentParent = contentParent;
            _mainInventoryPanel = mainInventoryPanel;
            _characterInventoryPanel = characterInventoryPanel;
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uiManager = GameClient.Get<IUIManager>();

            Init(contentParent);
        }

        public void Dispose()
        {
            _mainPanelToggle.onValueChanged.RemoveListener(OnMainPanelToggleChanged);
            _characterPanelToggle.onValueChanged.RemoveListener(OnCharacterPanelToggleChanged);
            _backButton.onClick.RemoveListener(BackButtonHandler);
            if(_selfObject!=null) 
                MonoBehaviour.Destroy(_selfObject);
        }

        private void Init(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_tabsPanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();
            _mainPanelToggle = _selfObject.transform.Find("Tabs/Toggle_MainTab").GetComponent<Toggle>();
            _characterPanelToggle = _selfObject.transform.Find("Tabs/Toggle_CharacterTab").GetComponent<Toggle>();
            _backButton = _selfObject.transform.Find("Button_Back").GetComponent<Button>();
            
            _mainPanelToggle.onValueChanged.AddListener(OnMainPanelToggleChanged);
            _characterPanelToggle.onValueChanged.AddListener(OnCharacterPanelToggleChanged);
            _backButton.onClick.AddListener(BackButtonHandler);
            _characterInventoryPanel.Show(true);
            _mainInventoryPanel.Show(false);
        }

        private void BackButtonHandler()
        {
            _uiManager.SetPage<MainPage>();
            _userProfileManager.Inventory?.SendChangesToServer();
        }

        private void OnMainPanelToggleChanged(bool isOn)
        {
            if (!isOn) return;
            _mainInventoryPanel.Show(true);
            _characterInventoryPanel.Show(false);
        }

        private void OnCharacterPanelToggleChanged(bool isOn)
        {
            if (!isOn) return;
            _characterInventoryPanel.Show(true);
            _mainInventoryPanel.Show(false);
        }

        public void SetFirstTabOn() => _characterPanelToggle.isOn = true;
    }
}