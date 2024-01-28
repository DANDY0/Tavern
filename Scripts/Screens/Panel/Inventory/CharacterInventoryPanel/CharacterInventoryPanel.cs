using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static GrandDevs.Tavern.Common.Enumerators;

namespace GrandDevs.Tavern
{
    public class CharacterInventoryPanel
    {
        private readonly IGameplayManager _gameplayManager;
        private readonly IUserProfileManager _userProfileManager;
        private readonly IDataManager _dataManager;
        private readonly GameplayController _gameplayController;
        private readonly ModelInputController _inputModelHandler;
        private GameObject _selfObject;
        private Transform _contentParent;
        private CharacterInventoryView _characterInventoryView;
        private PlayerModelView _playerModelView;
        private ChangeCharacterView _changeCharacterView;
        
        private readonly string _characterInventoryPanelPath = "Prefabs/UI/InventoryPageUI/CharacterInventoryPanel/CharacterInventoryPanel";

        public CharacterInventoryPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _inputModelHandler = GameClient.Get<IGameplayManager>().GetController<ModelInputController>();

            Init(contentParent);
        }
        
        public void Show(bool state)
        {
            _selfObject.SetActive(state);
            if (state)
            {
                _inputModelHandler.SetMainModelPosition();
                _inputModelHandler.SetCurrentModelInventory(_dataManager.CachedUserLocalData.lastChosenCharacterInventory);
            }
        }

        public void Dispose()
        {
            // _inventoryManager.Inventory.OnInventoryUpdated -= InventoryUpdatedHandler;
            _changeCharacterView.Dispose();
            _playerModelView.Dispose();
            if(_selfObject!=null)
                MonoBehaviour.Destroy(_selfObject);
        }

        private void Init(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_characterInventoryPanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();
            _userProfileManager.Inventory.InventoryUpdatedEvent += InventoryUpdatedEventHandler;
            _playerModelView = new PlayerModelView(_selfObject.transform.Find("PlayerModelView").gameObject);
            _characterInventoryView = new CharacterInventoryView(_selfObject.transform.Find("ArtefactsInventoryView").gameObject, _playerModelView);
            _changeCharacterView = new ChangeCharacterView(_selfObject.transform.Find("PlayerModelView/ChangeCharacterView").gameObject);
        }

        private void InventoryUpdatedEventHandler()
        {
            _characterInventoryView.InventoryUpdated(_userProfileManager.Inventory.GetItemsByType(ItemType.Artifact));
        }

    }
}