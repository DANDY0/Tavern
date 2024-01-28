using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    class CharacterItem : InventoryItemBase
    {
        public event Action<Inventory.ItemData> OnEquipClicked;
        protected float _popupDelay = 1;

        public CharacterItem(GameObject selfObject, Inventory.ItemData itemData, BaseItemPopup itemInfoPopup)
            : base(selfObject, itemData, itemInfoPopup) { }

        protected override InventoryItemView CreateView(GameObject selfObject)
        {
            return new CharacterItemView(selfObject, this);
        }

        public void ReturnToInventory() 
            => ((CharacterItemView)_inventoryItemView).ReturnToInventory();

        private void Equip()
        {
            OnEquipClicked?.Invoke(ItemData);
            _itemInfoPopup.Hide();
            _showPopupSequence?.Kill();
        }

        protected override void OnPointerEnter(BaseEventData eventData)
        {
            base.OnPointerEnter(eventData);

            _showPopupSequence = DOTween.Sequence()
                .AppendInterval(_popupDelay)
                .AppendCallback(() => _itemInfoPopup.Show(ItemData.ArtifactType.ToString(), _inventoryItemView.GetPanelText()))
                .SetId(this);
        }
        protected override void OnPointerExit(BaseEventData eventData)
        {
            base.OnPointerExit(eventData);

            _itemInfoPopup.Hide();
            _showPopupSequence?.Kill();        
        }

        class CharacterItemView : InventoryItemView
        {
            private GameObject _selfObject;
            private readonly CharacterItem _characterItem;

            public CharacterItemView(GameObject selfObject, CharacterItem characterItem) : base(selfObject)
            {
                _characterItem = characterItem;
                _selfObject = selfObject;
                _actionButton = selfObject.transform.Find("Button_Equip").GetComponent<Button>();
                _actionButton.onClick.AddListener(Equip);
            }

            public void Dispose() => _actionButton.onClick.RemoveListener(Equip);

            public void ReturnToInventory() => 
                SetEnable(true);

            private void Equip()
            {
                _characterItem.Equip();
                Deselect();
                SetEnable(false);
            }

            private void SetEnable(bool state) => _selfObject.SetActive(state);
        }

    }
}