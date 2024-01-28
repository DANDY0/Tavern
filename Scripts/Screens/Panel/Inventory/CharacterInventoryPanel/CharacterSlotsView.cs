using System;
using System.Collections.Generic;
using System.Linq;
using GrandDevs.Tavern.Common;
using UnityEngine;
using UnityEngine.UI;

namespace GrandDevs.Tavern
{
    public class CharacterSlotsView
    {
        private readonly ILoadObjectsManager _loadObjectsManager;

        private Dictionary<Enumerators.ArtifactType, CharacterItem> _equippedArtefacts =
            new Dictionary<Enumerators.ArtifactType, CharacterItem>();

        private CharacterInventoryView _characterInventoryView;
        private InventoryItemsData _inventoryItemsData;
        private PlayerModelView _playerModelView;
        private GameObject _selfObject;

        private Dictionary<Enumerators.ArtifactType, CharacterSlotItemView> _artefactsButtons
            = new Dictionary<Enumerators.ArtifactType, CharacterSlotItemView>();

        public CharacterSlotsView(GameObject selfObject, Transform backSlotsParent, PlayerModelView playerModelView)
        {
            _selfObject = selfObject;
            _playerModelView = playerModelView;
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _inventoryItemsData = _loadObjectsManager.GetObjectByPath<InventoryItemsData>("ScriptableObjects/InventoryItemsData");

            InitSlots(backSlotsParent);
        }

        private void InitSlots(Transform backSlotsParent)
        {
            foreach (Enumerators.ArtifactType artefactType in Enum.GetValues(typeof(Enumerators.ArtifactType)))
            {
                var button = _selfObject.transform.Find($"{artefactType}").GetComponent<Button>();
                var image = backSlotsParent.Find($"{artefactType}Back/Image_Icon").GetComponent<Image>();

                button.onClick.AddListener(() => _playerModelView.UnEquipInvoke(artefactType));
                _artefactsButtons[artefactType] = new CharacterSlotItemView(button, image);
            }
        }

        public void EquipItem(Inventory.ItemData itemData)
        {
            _artefactsButtons[itemData.ArtifactType].EquipInSlot(GetArtefactData(itemData));
        }

        public void UnEquipItem(Enumerators.ArtifactType artifactType)
        {
            _artefactsButtons[artifactType].UnEquipSlot();
        }

        private ArtefactItemData GetArtefactData(Inventory.ItemData item)
        {
            return _inventoryItemsData.ArtefactItemsData.FirstOrDefault(a => a.Type == item.ArtifactType);
        }

        class CharacterSlotItemView
        {
            private readonly GameObject _selfObject;
            private readonly Button _slotButton;
            private readonly Image _slotBackIcon;
            private readonly Image _slotIcon;

            private ArtefactItemData _artefactData;
            private int _index;
            private bool _isSlotFull;

            public CharacterSlotItemView(Button slotButton, Image slotBackIcon)
            {
                _selfObject = slotButton.gameObject;
                _slotButton = slotButton;
                _slotBackIcon = slotBackIcon;
                _slotIcon = slotButton.transform.Find("Image_Icon").GetComponent<Image>();
            }

            public void EquipInSlot(ArtefactItemData artefactItem)
            {
                _slotBackIcon.enabled = false;
                _slotIcon.enabled = true;
                _artefactData = artefactItem;
                _slotIcon.sprite = artefactItem.VisualData.ItemSprite;
                _isSlotFull = true;
            }

            public void UnEquipSlot()
            {
                if (!_isSlotFull)
                    return;
                _isSlotFull = false;
                _slotBackIcon.enabled = true;
                _slotIcon.enabled = false;
                _slotIcon.sprite = null;
                _artefactData = null;
            }
        }

        public void Dispose()
        {
        }
    }
}