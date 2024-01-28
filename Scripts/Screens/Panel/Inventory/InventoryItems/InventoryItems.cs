using System;
using System.Linq;
using DG.Tweening;
using GrandDevs.Tavern.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    abstract class InventoryItemBase
    {
        public event Action<InventoryItemBase> OnSelected;
        public ItemVisualDataBase ItemVisualDataBase;
        public Inventory.ItemData ItemData;
        
        protected GameObject _selfObject;
        protected InventoryItemView _inventoryItemView;
        protected Button _itemButton;
        protected BaseItemPopup _itemInfoPopup;
        protected Sequence _showPopupSequence;
        
        protected InventoryItemBase(GameObject selfObject, Inventory.ItemData itemData, BaseItemPopup itemInfoPopup)
        {
            ItemData = itemData;
            _selfObject = selfObject;
            _itemButton = _selfObject.GetComponent<Button>();
            
            _itemButton.onClick.AddListener(Select);
            _inventoryItemView = CreateView(_selfObject);
            _itemInfoPopup = itemInfoPopup;

            AddEventTrigger(_selfObject, EventTriggerType.PointerEnter, OnPointerEnter);
            AddEventTrigger(_selfObject, EventTriggerType.PointerExit, OnPointerExit);

        }

        public void Select()
        {
            _inventoryItemView.Select();
            OnSelected?.Invoke(this);
        }

        public void Deselect()
        {
            _inventoryItemView.Deselect();
        }

        public void Dispose()
        {
            _itemButton.onClick.RemoveListener(Select);
            if(_selfObject!=null)
                MonoBehaviour.Destroy(_selfObject);
        }

        public void DestroyItem() => MonoBehaviour.Destroy(_selfObject);
        public void SetVisual() => _inventoryItemView.SetVisual(ItemData);

        private void AddEventTrigger(GameObject target, EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>() ?? target.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }

        protected virtual void OnPointerEnter(BaseEventData eventData)
        {
            _itemInfoPopup.SetPosition(_selfObject.GetComponent<RectTransform>());
        }

        protected virtual void OnPointerExit(BaseEventData eventData)
        {
            _itemInfoPopup.Hide();
        }
        
        protected abstract InventoryItemView CreateView(GameObject selfObject);
    }

    class InventoryItemView
    {
        private GameObject _selfObject;
        protected readonly ILoadObjectsManager _loadObjectsManager;
        protected Image _itemIcon;
        protected Button _actionButton;
        protected InventoryItemsData _inventoryItemsData;
        protected ItemVisualDataBase _itemVisualDataBase;

        public InventoryItemView(GameObject selfObject)
        {
            _selfObject = selfObject;
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _inventoryItemsData = _loadObjectsManager.GetObjectByPath<InventoryItemsData>("ScriptableObjects/InventoryItemsData");
            _itemIcon = _selfObject.transform.Find("Image_Icon").GetComponent<Image>();
        }
        
        public void Select()
        {
            _actionButton.gameObject.SetActive(true);
        }

        public void Deselect()
        {
            _actionButton.gameObject.SetActive(false);
        }

        public void Dispose()
        {
        }

        public void SetVisual(Inventory.ItemData itemData)
        {
            SetVisualData(itemData);
            Sprite itemSprite = _itemVisualDataBase.VisualData.ItemSprite;
            _itemIcon.sprite = itemSprite;
        }

        private void SetVisualData(Inventory.ItemData item)
        {
            ItemVisualDataBase itemVisualDataBase = null;
            if (item.type == Enumerators.ItemType.Artifact) 
                itemVisualDataBase = GetArtefactData(item);
            if (item.type == Enumerators.ItemType.Potion || item.type == Enumerators.ItemType.ScrollOfTeleport)
                itemVisualDataBase = GetMainData(item);
            
            _itemVisualDataBase = itemVisualDataBase;
        }

        public string GetPanelText() => _itemVisualDataBase.VisualData.Description;

        private ArtefactItemData GetArtefactData(Inventory.ItemData item) 
            => _inventoryItemsData.ArtefactItemsData.FirstOrDefault(a => a.Type == item.ArtifactType);
        private MainItemData GetMainData(Inventory.ItemData item) 
            => _inventoryItemsData.MainItemsData.FirstOrDefault(a => a.Types == item.type);

    }
    
}