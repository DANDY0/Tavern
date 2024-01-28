using System.Collections.Generic;
using GrandDevs.Tavern.Common;

namespace GrandDevs.Tavern
{
    class CharacterEquipment
    {
        private readonly Inventory _inventory;
        private List<Dictionary<Enumerators.ArtifactType, Inventory.ItemData>> _charactersEquipment;

        public CharacterEquipment()
        {
            _inventory = GameClient.Get<IUserProfileManager>().Inventory;
            _inventory.InventoryUpdatedEvent += InitEquipment;
        }

        private void InitEquipment()
        {
            _charactersEquipment = new List<Dictionary<Enumerators.ArtifactType, Inventory.ItemData>>();
            var charactersCount = _inventory.GetCharacters().Count;
            for (int i = 0; i < charactersCount; i++)
                _charactersEquipment.Add(new Dictionary<Enumerators.ArtifactType, Inventory.ItemData>());
        }

        public bool IsSlotFull(Enumerators.ArtifactType artifactType, int characterID)
        {
            return _charactersEquipment[characterID].ContainsKey(artifactType) 
                   && _charactersEquipment[characterID][artifactType] != null;
        }

        public void EquipItem(Inventory.ItemData itemData, int characterID) 
            => _charactersEquipment[characterID][itemData.ArtifactType] = itemData;

        public void UnEquipItem(Enumerators.ArtifactType artifactType, int characterID) 
            => _charactersEquipment[characterID][artifactType] = null;

        public Inventory.ItemData GetItemByType(Enumerators.ArtifactType artifactType, int currentCharacterId)
        {
            return _charactersEquipment[currentCharacterId][artifactType];
        }

        public void Dispose()
        {
            _inventory.InventoryUpdatedEvent -= InitEquipment;
        }
    }
}