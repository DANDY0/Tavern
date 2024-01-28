using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class MainInventoryPanel
    {
        private IGameplayManager _gameplayManager;
        private IUserProfileManager _userProfileManager;
        private GameplayController _gameplayController;
        private GameObject _selfObject;
        private Transform _contentParent;
        private Transform _filterButtonsParent;
        
        private MainInventoryView _mainInventoryView;

        private readonly string _mainInventoryPanelPath = "Prefabs/UI/InventoryPageUI/MainInventoryPanel/MainInventoryPanel";
        
        public MainInventoryPanel(Transform contentParent)
        {
            _contentParent = contentParent;
            _userProfileManager = GameClient.Get<IUserProfileManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            Init(contentParent);
        }

        public void Show(bool state) => _selfObject.SetActive(state);

        public void Dispose()
        {
            if(_selfObject!=null)
                MonoBehaviour.Destroy(_selfObject);
        }
        
        private void Init(Transform contentParent)
        {
            _selfObject = MonoBehaviour.Instantiate(Resources.Load<GameObject>(_mainInventoryPanelPath), contentParent);
            _selfObject.transform.SetAsFirstSibling();
            _userProfileManager.Inventory.InventoryUpdatedEvent += InventoryUpdatedEventHandler;
            _mainInventoryView = new MainInventoryView(_selfObject.transform.Find("MainInventoryView").gameObject);
        }
        
        private void InventoryUpdatedEventHandler()
        {
            List<Inventory.ItemData> mainInventoryItems = _userProfileManager.Inventory.GetAllMainItems();
            _mainInventoryView.InventoryUpdated(mainInventoryItems);
        }
    }

   
}