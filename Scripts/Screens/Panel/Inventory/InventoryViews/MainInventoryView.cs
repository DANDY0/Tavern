using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    class MainInventoryView : InventoryViewBase
    {
        protected List<MainItem> _mainItems = new List<MainItem>();
        private MainFilterView _mainFilterView;
        private MainInventoryPopup _itemInfoPopup;

        public MainInventoryView(GameObject selfObject) : base(selfObject)
        {
            _mainFilterView = new MainFilterView(_selfObject.transform.Find("FilterButtonsView").gameObject, this);
            _itemInfoPopup = new MainInventoryPopup(_selfObject.transform.Find("ItemInfoPopupMain").gameObject);
        }

        protected override InventoryItemBase CreateInventoryItem(GameObject itemObject, Inventory.ItemData itemData)
        {
            var mainItem = new MainItem(itemObject, itemData, _itemInfoPopup);
            mainItem.SetVisual();
            _mainItems.Add(mainItem);
            return mainItem;
        }

        private void SetItemsVisibilityByType(List<Inventory.ItemData> itemsToShow)
        {
            foreach (var item in _mainItems) 
                item.SetEnable(false);

            if (itemsToShow != null)
            {
                foreach (var itemData in itemsToShow)
                {
                    MainItem mainItem = FindUIItemByIndex(itemData.id);
                    mainItem.SetEnable(true);
                }
            }
        }

        private MainItem FindUIItemByIndex(string id)
        {
            return _mainItems.FirstOrDefault(item => item.ItemData.id == id);
        }
        
        class MainFilterView
        {
            private readonly IUserProfileManager _userProfileManager;
            private GameObject _selfObject;
            private MainInventoryView _mainInventoryView;
            private Dictionary<Enumerators.FilterButtonTypes, Button> _filterButtons = new Dictionary<Enumerators.FilterButtonTypes, Button>();
            private Dictionary<Enumerators.FilterButtonTypes, Image> _filterImages = new Dictionary<Enumerators.FilterButtonTypes, Image>();

            private Color _selectColor = new Color(0.96f, 0.76f, 0.23f);

            public MainFilterView(GameObject selfObject, MainInventoryView mainInventoryView)
            {
                _selfObject = selfObject;
                _userProfileManager = GameClient.Get<IUserProfileManager>();
                _mainInventoryView = mainInventoryView;
                foreach (Enumerators.FilterButtonTypes filterType in Enum.GetValues(typeof(Enumerators.FilterButtonTypes)))
                {
                    var button = _selfObject.transform.Find($"Button_{filterType}").GetComponent<Button>();
                    var image = _selfObject.transform.Find($"Button_{filterType}/Image_Back").GetComponent<Image>();
                    button.onClick.AddListener(() => OnFilterButtonClicked(filterType));
                    _filterButtons[filterType] = button;
                    _filterImages[filterType] = image;
                }
                _filterImages[Enumerators.FilterButtonTypes.All].color = _selectColor;
            }
            
            private void OnFilterButtonClicked(Enumerators.FilterButtonTypes filterType)
            {
                foreach (var image in _filterImages) 
                    image.Value.color = Color.white;
                _filterImages[filterType].color = _selectColor;
                    
                List<Inventory.ItemData> filteredItems;
            
                if (filterType == Enumerators.FilterButtonTypes.All)
                    filteredItems = _userProfileManager.Inventory.GetAllMainItems();
                else
                {
                    Enumerators.ItemType itemType = InternalTools.EnumFromString<Enumerators.ItemType>(filterType.ToString());
                    filteredItems = _userProfileManager.Inventory.GetItemsByType(itemType);
                }
                
                _mainInventoryView.SetItemsVisibilityByType(filteredItems);
            }

        }
    }
}