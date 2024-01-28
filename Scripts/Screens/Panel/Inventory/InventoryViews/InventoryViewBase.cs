using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
    abstract class InventoryViewBase
    {
        protected GameObject _selfObject;
        protected Transform _inventorySlotsParent;
        protected InventoryItemBase _currentlySelected;
        protected GameObject _prefabItem;

        protected List<InventoryItemBase> _inventoryItems = new List<InventoryItemBase>();
        
        public InventoryViewBase(GameObject selfObject)
        {
            _selfObject = selfObject;
            var loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _inventorySlotsParent = _selfObject.transform.Find("InventorySlots");
            _prefabItem = loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/InventoryPageUI/CharacterInventoryPanel/InventoryItem");
        }

        public virtual void InventoryUpdated(List<Inventory.ItemData> items)
        {
            if (_inventoryItems != null && _inventoryItems.Count > 0)
            {
                foreach (var item in _inventoryItems)
                {
                    item.OnSelected -= HandleItemSelected;
                    item.DestroyItem();
                    item.Dispose();
                }
            }
            _inventoryItems.Clear();

            foreach (var itemData in items)
            {
                var itemObject = MonoBehaviour.Instantiate(_prefabItem, _inventorySlotsParent);
                var inventoryItem = CreateInventoryItem(itemObject, itemData);
                _inventoryItems.Add(inventoryItem);
                inventoryItem.OnSelected += HandleItemSelected;
            }
        }

        public virtual void Dispose()
        {
            foreach (var item in _inventoryItems)
                item.OnSelected -= HandleItemSelected;
            _inventoryItems.Clear();
        }

        protected abstract InventoryItemBase CreateInventoryItem(GameObject itemObject, Inventory.ItemData itemData);

        protected void HandleItemSelected(InventoryItemBase item)
        {
            if (_currentlySelected == null)
            {
                _currentlySelected = item;
                return;
            }

            if (_currentlySelected == item)
            {
                _currentlySelected.Deselect();
                _currentlySelected = null;
                return;
            }

            _currentlySelected.Deselect();
            _currentlySelected = item;
        }
    }
}