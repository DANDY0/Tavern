using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    class MainItem : InventoryItemBase
    {
        protected float _popupDelay = 1;
        public MainItem(GameObject selfObject, Inventory.ItemData itemData, BaseItemPopup itemInfoPopup)
            : base(selfObject, itemData, itemInfoPopup) { }
        protected override InventoryItemView CreateView(GameObject selfObject)
        {
            return new MainItemView(selfObject);
        }

        protected override void OnPointerEnter(BaseEventData eventData)
        {
            base.OnPointerEnter(eventData);

            _showPopupSequence = DOTween.Sequence()
                .AppendInterval(_popupDelay)
                .AppendCallback(() => _itemInfoPopup.Show(ItemData.type.ToString(), _inventoryItemView.GetPanelText()))
                .SetId(this);
        }
        protected override void OnPointerExit(BaseEventData eventData)
        {
            base.OnPointerExit(eventData);

            _itemInfoPopup.Hide();
            _showPopupSequence?.Kill();        
        }

        public void SetEnable(bool state) => _selfObject.SetActive(state);

        class MainItemView : InventoryItemView
        {
            public MainItemView(GameObject selfObject) : base(selfObject)
            {
                _actionButton = selfObject.transform.Find("Button_Use").GetComponent<Button>();
                _actionButton.onClick.AddListener(Use);
            }

            private void Use()
            {
            }
            
            public void Dispose() => _actionButton.onClick.RemoveListener(Use);
        }
    }
}