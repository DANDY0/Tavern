using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class ChangeCharacterView
    {
        private GameObject _selfObject;
        private readonly IDataManager _dataManager;
        private readonly ILoadObjectsManager _loadObjectsManager;
        private ModelInputController _modelInputController;

        private Inventory _inventory;
        private List<int> _openedCharacters;
        private Dictionary<int, Sprite> _avaSprites;

        private Image _characterIcon;
        private Button _leftButton;
        private Button _rightButton;
        private int _currentCharacterIndex = 0;

        public ChangeCharacterView(GameObject selfObject)
        {
            _selfObject = selfObject;
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _modelInputController = GameClient.Get<IGameplayManager>().GetController<ModelInputController>();
            _inventory = GameClient.Get<IUserProfileManager>().Inventory;
            
            _characterIcon = _selfObject.transform.Find("CharacterIcon/Image_Icon").GetComponent<Image>();
            _leftButton = _selfObject.transform.Find("Button_Left").GetComponent<Button>();
            _rightButton = _selfObject.transform.Find("Button_Right").GetComponent<Button>();
            
            _inventory.InventoryUpdatedEvent += InventoryUpdatedEventEventHandler;
            _leftButton.onClick.AddListener(LeftButtonHandler);
            _rightButton.onClick.AddListener(RightButtonHandler);
        }

        public void Dispose()
        {
            _inventory.InventoryUpdatedEvent -= InventoryUpdatedEventEventHandler;
            _leftButton.onClick.RemoveListener(LeftButtonHandler);
            _rightButton.onClick.RemoveListener(RightButtonHandler);
        }

        private void InitData()
        {
            var characters = _inventory.GetCharacters();
            List<int> idCharacters = new List<int>();
            foreach (var character in characters)
                idCharacters.Add(character.stats.character);
            _openedCharacters = idCharacters;

            _avaSprites = new Dictionary<int, Sprite>();
            foreach (var character in _openedCharacters)
            {
                var sprite = _loadObjectsManager.GetObjectByPath<Sprite>($"Sprites/ava/{character}");
                _avaSprites.Add(character, sprite);
            }
        }

        private void InventoryUpdatedEventEventHandler()
        {
            _currentCharacterIndex = _dataManager.CachedUserLocalData.lastChosenCharacterInventory;
            InitData();
            UpdateCurrentCharacter();
        }

        private void LeftButtonHandler()
        {
            _currentCharacterIndex--;
            if (_currentCharacterIndex < 0)
                _currentCharacterIndex = _openedCharacters.Count - 1;
            UpdateCurrentCharacter();
        }

        private void RightButtonHandler()
        {
            _currentCharacterIndex++;
            if (_currentCharacterIndex >= _openedCharacters.Count) 
                _currentCharacterIndex = 0;
            UpdateCurrentCharacter();
        }

        private void UpdateCurrentCharacter()
        {
            int characterId = _openedCharacters[_currentCharacterIndex];
            _characterIcon.sprite = _avaSprites[characterId];
            _dataManager.CachedUserLocalData.lastChosenCharacterInventory = characterId;
            
            _modelInputController.SetCurrentModelInventory(characterId);
        }
    }
}