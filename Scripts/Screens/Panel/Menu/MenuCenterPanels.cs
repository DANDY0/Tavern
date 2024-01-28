using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class MenuCenterPanels
    {
        private IGameplayManager _gameplayManager;
        private IUserProfileManager _userProfileManager;
        private IUIManager _uiManager;
        private ModelInputController _modelInputController;
        private GameObject _selfObject;
        private Transform _contentParent;

        private ChooseCharacterPanel _chooseCharacterPanel;

        private Button _playGameButton;
        private Button _playGameWithBotsButton;
        private Button _playGameWithBotsButton_2;
        private Button _playGameWithBotsButton_3;

        private Button _inventoryButton;
        private Button _mapButton;

        private readonly string _chooseCharacterPanelPath = "Prefabs/UI/MainPageUI/MenuCenterPanel/CenterPanel";

        public MenuCenterPanels(Transform contentParent)
        {
            _contentParent = contentParent;
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _uiManager = GameClient.Get<IUIManager>();
            Init(contentParent);
        }

        public void Dispose()
        {
            _playGameButton.onClick.RemoveListener(PlayGameButtonOnClickHandler);
            _playGameWithBotsButton.onClick.RemoveListener(PlayGameWithBotsButtonOnClickHandler);
            _playGameWithBotsButton_2.onClick.RemoveListener(PlayGameWithBots2v2ButtonOnClickHandler);
            _playGameWithBotsButton_3.onClick.RemoveListener(PlayGameWithBots3v1ButtonOnClickHandler);
            _inventoryButton.onClick.AddListener(InventoryClicked);
            _chooseCharacterPanel.Dispose();
            _chooseCharacterPanel = null;
            if (_selfObject != null)
                MonoBehaviour.Destroy(_selfObject);
        }

        private void Init(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_chooseCharacterPanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();
            _chooseCharacterPanel = new ChooseCharacterPanel(_selfObject.transform, this);
            _playGameButton = _selfObject.transform.Find("ButtonsPlayPanel/Button_PlayGame").GetComponent<Button>();
            _playGameWithBotsButton = _selfObject.transform.Find("ButtonsPlayPanel/Button_PlayWithBots").GetComponent<Button>();
            _playGameWithBotsButton_2 = _selfObject.transform.Find("ButtonsPlayPanel/Button_PlayWithBots_2v2").GetComponent<Button>();
            _playGameWithBotsButton_3 = _selfObject.transform.Find("ButtonsPlayPanel/Button_PlayWithBots_3v1").GetComponent<Button>();
            _inventoryButton = _selfObject.transform.Find("LeftSidePanel/Button_Inventory").GetComponent<Button>();
            _mapButton = _selfObject.transform.Find("LeftSidePanel/Button_Map").GetComponent<Button>();

            _playGameButton.onClick.AddListener(PlayGameButtonOnClickHandler);
            _playGameWithBotsButton.onClick.AddListener(PlayGameWithBotsButtonOnClickHandler);
            _playGameWithBotsButton_2.onClick.AddListener(PlayGameWithBots2v2ButtonOnClickHandler);
            _playGameWithBotsButton_3.onClick.AddListener(PlayGameWithBots3v1ButtonOnClickHandler);
            _inventoryButton.onClick.AddListener(InventoryClicked);
            _mapButton.onClick.AddListener(MapClicked);
        }

        private void InventoryClicked() => _uiManager.SetPage<InventoryPage>();

        private void MapClicked() => _uiManager.SetPage<MapPage>();

        private void PlayGameButtonOnClickHandler()
        {
            if (!_userProfileManager.IsStaminaEnough())
                return;
            (GameClient.Get<INetworkManager>() as NetworkManager).mode = "Deathmatch";
            GameClient.Get<IGameplayManager>().GetController<ConnectionController>().FindFreeRoomAndJoinGame();
        }

        private void PlayGameWithBotsButtonOnClickHandler()
        {
            if (!_userProfileManager.IsStaminaEnough())
                return;
            (GameClient.Get<INetworkManager>() as NetworkManager).mode = "Deathmatch_Test";
            GameClient.Get<IGameplayManager>().GetController<ConnectionController>().FindFreeRoomAndJoinGame();
        }

        private void PlayGameWithBots2v2ButtonOnClickHandler()
        {
            if (!_userProfileManager.IsStaminaEnough())
                return;
            (GameClient.Get<INetworkManager>() as NetworkManager).mode = "Deathmatch2VS2";
            GameClient.Get<IGameplayManager>().GetController<ConnectionController>().FindFreeRoomAndJoinGame();
        }

        private void PlayGameWithBots3v1ButtonOnClickHandler()
        {
            if (!_userProfileManager.IsStaminaEnough())
                return;
            (GameClient.Get<INetworkManager>() as NetworkManager).mode = "Deathmatch3VS1";
            GameClient.Get<IGameplayManager>().GetController<ConnectionController>().FindFreeRoomAndJoinGame();
        }

        class ChooseCharacterPanel
        {
            private GameObject _selfObject;
            private readonly IGameplayManager _gameplayManager;
            private readonly IUserProfileManager _userProfileManager;
            private readonly IDataManager _dataManager;
            private readonly ModelInputController _modelInputController;
            private readonly MenuCenterPanels _menuCenterPanels;

            private List<CharacterProfileView> _charactersProfiles = new List<CharacterProfileView>();
            private CharacterProfileView _currentChosenCharacter;

            private Transform _contentParent;
            private int _charactersCount = 4;

            private readonly string _chooseCharacterPanelPath = "Prefabs/UI/MainPageUI/MenuCenterPanel/ChooseCharacterPanel";

            public ChooseCharacterPanel(Transform contentParent, MenuCenterPanels menuCenterPanels)
            {
                _gameplayManager = GameClient.Get<IGameplayManager>();
                _dataManager = GameClient.Get<IDataManager>();
                _userProfileManager = GameClient.Get<IUserProfileManager>();
                _modelInputController = _gameplayManager.GetController<ModelInputController>();
                _contentParent = contentParent;
                _menuCenterPanels = menuCenterPanels;
                Init();
            }

            private void Init()
            {
                _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_chooseCharacterPanelPath),
                    _contentParent);
                var characterIdIncrementor = 0;
                foreach (Transform child in _selfObject.transform)
                {
                    _charactersProfiles.Add(new CharacterProfileView(characterIdIncrementor, child.gameObject));
                    var character = _charactersProfiles.Last();
                    character.OnCharacterClicked += CharacterButtonClicked;
                    characterIdIncrementor++;
                }

                _userProfileManager.Inventory.InventoryUpdatedEvent += InventoryUpdatedEventEventHandler;
            }

            private void InventoryUpdatedEventEventHandler()
            {
                SelectDefaultCharacter();
                UnlockCharacters();
            }

            private void SelectDefaultCharacter()
            {
                int defaultCharacterID = _dataManager.CachedUserLocalData.lastChosenCharacterMenu;
                _charactersProfiles[defaultCharacterID].SetChosen(true);
                _modelInputController.SetCurrentModelMenu(defaultCharacterID);
                _currentChosenCharacter = _charactersProfiles[defaultCharacterID];
            }

            private void UnlockCharacters()
            {
                var itemDatas = _userProfileManager.Inventory.GetAllItems();
                if (itemDatas == null || itemDatas.Count < 1)
                    return;

                foreach (var character in _userProfileManager.Inventory.GetCharacters())
                {
                    _charactersProfiles[character.stats.character].SetLocked(false);
                    _charactersProfiles[character.stats.character].SetStamina((_userProfileManager as UserProfileManager).StaminaValue);
                }
            }

            private void CharacterButtonClicked(CharacterProfileView character)
            {
                if (_currentChosenCharacter != null) 
                    _currentChosenCharacter.SetChosen(false);

                _currentChosenCharacter = character;
                _currentChosenCharacter.SetChosen(true);
                _modelInputController.SetCurrentModelMenu(_currentChosenCharacter.GetCharacterID());

                _dataManager.CachedUserLocalData.lastChosenCharacterMenu = _currentChosenCharacter.GetCharacterID();
            }

            public void Dispose()
            {
                foreach (var profile in _charactersProfiles)
                    profile.OnCharacterClicked -= CharacterButtonClicked;
                _charactersProfiles = new List<CharacterProfileView>();
                _currentChosenCharacter = null;
            }

            class CharacterProfileView
            {
                private readonly GameObject _selfObject;
                private readonly int _characterID;
                private Button _characterButton;
                private Image _characterImage;
                private Image _characterChosenImage;
                private Image _staminaImage;
                private TextMeshProUGUI _staminaText;

                private GameConfig.CharacterData _characterData;

                public Action<CharacterProfileView> OnCharacterClicked;

                public CharacterProfileView(int characterID, GameObject selfObject)
                {
                    _characterData = new GameConfig.CharacterData();
                    _characterData.stamina = 100;
                    _characterID = characterID;
                    _selfObject = selfObject;
                    _characterButton = _selfObject.GetComponent<Button>();
                    _characterChosenImage = _selfObject.transform.Find("Visual/Image_Chosen").GetComponent<Image>();
                    _staminaImage = _selfObject.transform.Find("Visual/Image_Stamina").GetComponent<Image>();
                    _characterImage = _selfObject.transform.Find("Visual/Mask/Image_Icon").GetComponent<Image>();
                    _staminaText = _selfObject.transform.Find("Text_Stamina").GetComponent<TextMeshProUGUI>();

                    _characterButton.onClick.AddListener(() => OnCharacterClicked?.Invoke(this));
                    SetLocked(true);
                }

                public void SetLocked(bool state)
                {
                    _characterImage.color = state ? Color.gray : Color.white;
                    _characterButton.interactable = !state;
                }

                public int GetCharacterID() => _characterID;

                public void SetChosen(bool state) => _characterChosenImage.enabled = state;

                public void Dispose() =>
                    _characterButton.onClick.RemoveListener(() => OnCharacterClicked?.Invoke(this));

                public void SetStamina(int value)
                {
                    _staminaImage.fillAmount = value * 0.01f;
                    _staminaText.text = $"{value}%";
                    var inventoryManager = GameClient.Get<IUserProfileManager>();
                    SetLocked(inventoryManager.IsStaminaEnough() != true);
                }
            }
        }
    }
}