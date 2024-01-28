using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
    class CharacterInventoryView : InventoryViewBase
    {
        private readonly PlayerModelView _playerModelView;
        protected List<CharacterItem> _characterItems = new List<CharacterItem>();
        private CharacterInventoryPopup _itemInfoPopup;

        public CharacterInventoryView(GameObject selfObject, PlayerModelView playerModelView) : base(selfObject)
        {
            _playerModelView = playerModelView;
            _playerModelView.OnUnEquipItem += ReturnToInventory;
            _itemInfoPopup = new CharacterInventoryPopup(_selfObject.transform.Find("ItemInfoPopupCharacter").gameObject);

        }

        protected override InventoryItemBase CreateInventoryItem(GameObject itemObject, Inventory.ItemData itemData)
        {
            var characterItem = new CharacterItem(itemObject, itemData, _itemInfoPopup);
            characterItem.SetVisual();
            characterItem.OnEquipClicked += EquipClickedEventHandler;
            _characterItems.Add(characterItem);
            return characterItem;
        }

        private void EquipClickedEventHandler(Inventory.ItemData itemData) 
            => _playerModelView.EquipInvoke(itemData);

        private void ReturnToInventory(Inventory.ItemData itemData)
        {
            var characterItem = _characterItems.Find(i => i.ItemData.id == itemData.id);
            characterItem.ReturnToInventory();
            _currentlySelected = null;
        }
    }
}