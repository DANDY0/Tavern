using System;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public class PlayerModelView
    {
        private GameObject _selfObject;
        private Inventory _inventory;
        private CharacterSlotsView _characterSlotsView;
        private CharacterEquipment _equipment;
        private int _currentCharacterId;

        public event Action<Inventory.ItemData> OnEquippedItem;
        public event Action<Inventory.ItemData> OnUnEquipItem;


        public PlayerModelView(GameObject selfObject)
        {
            _selfObject = selfObject;
            _inventory = GameClient.Get<IUserProfileManager>().Inventory;
            _equipment = new CharacterEquipment();
            _characterSlotsView = new CharacterSlotsView(_selfObject.transform.Find("ArtefactsSlots").gameObject,
                _selfObject.transform.Find("ArtefactsSlotsBack"), this);
            _inventory.OnInventoryCharacterChanged += OnInventoryCharacterChanged;
            OnEquippedItem += EquipItem;
            OnUnEquipItem += UnEquipItem;
        }
        
        public void UnEquipInvoke(Enumerators.ArtifactType artifactType)
        {
            var itemData = _equipment.GetItemByType(artifactType, _currentCharacterId);
            if(itemData == null)
                return;
            OnUnEquipItem?.Invoke(itemData);
        }

        public void EquipInvoke(Inventory.ItemData itemData)
        {
            OnEquippedItem?.Invoke(itemData);
        }

        public void Dispose()
        {
            _characterSlotsView.Dispose();
            _equipment.Dispose();
            _inventory.OnInventoryCharacterChanged -= OnInventoryCharacterChanged;
            OnEquippedItem -= EquipItem;
            OnUnEquipItem -= UnEquipItem;
        }

        private void OnInventoryCharacterChanged(int character)
        {
            foreach (var artefactType in Enum.GetValues(typeof(Enumerators.ArtifactType)))
                if (_equipment.IsSlotFull((Enumerators.ArtifactType)artefactType, _currentCharacterId))
                    _characterSlotsView.UnEquipItem((Enumerators.ArtifactType)artefactType);
            
            _currentCharacterId = character;

            foreach (var artefactType in Enum.GetValues(typeof(Enumerators.ArtifactType)))
            {
                if(!_equipment.IsSlotFull((Enumerators.ArtifactType)artefactType, _currentCharacterId))
                    continue;
                var itemData = _equipment.GetItemByType((Enumerators.ArtifactType)artefactType, _currentCharacterId);
                if (itemData != null) 
                    _characterSlotsView.EquipItem(itemData );
            }
        }
        
        private void EquipItem(Inventory.ItemData itemData)
        {
            if (_equipment.IsSlotFull(itemData.ArtifactType, _currentCharacterId))
                UnEquipInvoke(itemData.ArtifactType);

            _equipment.EquipItem(itemData, _currentCharacterId);
            _characterSlotsView.EquipItem(itemData);

            _inventory.AddChangedItem(Inventory.ChangeType.Equip, itemData);
        }

        private void UnEquipItem(Inventory.ItemData itemData)
        {
            _equipment.UnEquipItem(itemData.ArtifactType, _currentCharacterId);
            _characterSlotsView.UnEquipItem(itemData.ArtifactType);
            
            _inventory.AddChangedItem(Inventory.ChangeType.Unequip, itemData);
        }

    }
}