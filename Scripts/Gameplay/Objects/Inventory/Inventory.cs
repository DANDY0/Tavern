using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrandDevs.Tavern.Common;
using GrandDevs.Tavern.Helpers;
using static External.Essentials.GrandDevs.SocketIONetworking.Scripts.APIModel.GetInventoryResponse;
using Random = UnityEngine.Random;

namespace GrandDevs.Tavern
{
    public class Inventory
    {
        public event Action InventoryUpdatedEvent;
        public event Action<int> OnInventoryCharacterChanged;
        public bool IsInventoryInitialized;

        private readonly INetworkManager _networkManager;
        private readonly Enumerators.ItemType[] _mainItemTypes = { Enumerators.ItemType.Potion, Enumerators.ItemType.ScrollOfTeleport, };
        
        private List<ResponseItemData> _responseItems = new List<ResponseItemData>();
        private List<ItemData> _inventoryItems = new List<ItemData>();
        private List<ItemChange> _changes = new List<ItemChange>();

        public Inventory()
        {
            _networkManager = GameClient.Get<INetworkManager>();
            (_networkManager as NetworkManager).APIRequestHandler.InventoryReceivedEvent += GetInventoryHandler;
        }

        public void Dispose()
        {
            (_networkManager as NetworkManager).APIRequestHandler.InventoryReceivedEvent -= GetInventoryHandler;
        }

        public List<ResponseItemData> GetAllResponseItems() => _responseItems;
        
        public List<ItemData> GetAllItems() => _inventoryItems;

        public List<ItemData> GetAllMainItems()
        {
            return GetAllItems().Where(item => _mainItemTypes.Contains(item.type)).ToList();
        }

        public List<ItemData> GetItemsByType(Enumerators.ItemType itemType)
        {
            return _inventoryItems.Where(item => item.type == itemType).ToList();
        }

        public ItemData GetFirstItem() => _inventoryItems.Count > 0 ? _inventoryItems[0] : null;

        public List<ItemData> GetCharacters() 
            => _inventoryItems.Where(i => i.type == Enumerators.ItemType.Character).ToList();

        public void TriggerChangedCharacter(int characterID)
        {
            OnInventoryCharacterChanged?.Invoke(characterID);
        }

        public void AddChangedItem(ChangeType changeType, ItemData itemData)
        {
            var existingChange = _changes.FirstOrDefault(ch => ch.Item.id == itemData.id);

            if (existingChange != null)
            {
                if (IsSameChange(changeType, existingChange))
                    _changes.Remove(existingChange);
                else
                    existingChange.ChangeType = changeType;
            }
            else
            {
                _changes.Add(new ItemChange
                {
                    ChangeType = changeType,
                    Item = itemData
                });
            }
        }

        private static bool IsSameChange(ChangeType changeType, ItemChange existingChange)
        {
            return (existingChange.ChangeType == ChangeType.Equip && changeType == ChangeType.Unequip) ||
                   (existingChange.ChangeType == ChangeType.Unequip && changeType == ChangeType.Equip);
        }
        
        public async Task SendChangesToServer()
        {
            foreach (var change in _changes)
            {
                switch (change.ChangeType)
                {
                    case ChangeType.Equip:
                        await EquipOnServer(change.Item);
                        break;

                    case ChangeType.Unequip:
                        await UnequipOnServer(change.Item);
                        break;
                }
            }

            _changes.Clear();
        }

        private async Task EquipOnServer(ItemData item)
        {
            var networkManager = (_networkManager as NetworkManager);
            await networkManager.APIRequestHandler.EquipArtifactAsync(item.id, item.owner);
        }

        private async Task UnequipOnServer(ItemData item)
        {
            var networkManager = (_networkManager as NetworkManager);
            await networkManager.APIRequestHandler.UnEquipArtifactAsync(item.id, item.owner);
        }
        
        private void GetInventoryHandler(List<ResponseItemData> items)
        {
            _responseItems = items;

             _inventoryItems = new List<ItemData>();
            _responseItems.ForEach(element => 
            {
                 _inventoryItems.Add(new ItemData
                {
                    id = element.id,
                    type = (Enumerators.ItemType)System.Enum.Parse(typeof(Enumerators.ItemType), element.type),
                    stats = new ItemStats
                    {
                        character = element.stats.character,
                    }
                });
            });
            
            //AddFakeItems();
            InventoryUpdatedEvent?.Invoke();
            IsInventoryInitialized = true;
        }

        private void AddFakeItems()
        {
            List<ItemData> newItems = new List<ItemData>();
            newItems.Add(new ItemData
            {
                id = "123",
                type = Enumerators.ItemType.Character,
                stats = new ItemStats
                {
                    character = 0,
                }
            });
            newItems.Add(new ItemData
            {
                id = "12364564",
                type = Enumerators.ItemType.Character,
                stats = new ItemStats
                {
                    character = 1,
                }
            });
            newItems.Add(new ItemData
            {
                id = "12364564890",
                type = Enumerators.ItemType.Character,
                stats = new ItemStats
                {
                    character = 2,
                }
            });
            newItems.Add(new ItemData
            {
                id = "123fsd",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Helmet,
            });
            newItems.Add(new ItemData
            {
                id = "123asfsd",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Hands,
            });
            newItems.Add(new ItemData
            {
                id = "1fsdf23asfsd",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Shield,
            });
            newItems.Add(new ItemData
            {
                id = "1fsdf23asf",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Necklace,
            });
            newItems.Add(new ItemData
            {
                id = "1fsd3asf",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Necklace,
            });
            newItems.Add(new ItemData
            {
                id = "1fsd3asfkujj",
                owner = "",
                type = Enumerators.ItemType.Artifact,
                ArtifactType = Enumerators.ArtifactType.Shield,
            });
            newItems.Add(new ItemData
            {
                id = "1fsd3asfkujjgfds",
                owner = "",
                type = Enumerators.ItemType.Potion,
            });
            newItems.Add(new ItemData
            {
                id = "1fsd3asfkhjnghds",
                owner = "",
                type = Enumerators.ItemType.ScrollOfTeleport,
            });
            newItems.Add(new ItemData
            {
                id = "3asfkhjnghdsghjgf",
                owner = "",
                type = Enumerators.ItemType.Potion,
            });
            newItems.Add(new ItemData
            {
                id = "3asfkhjnghds",
                owner = "",
                type = Enumerators.ItemType.Potion,
            });
            newItems.Add(new ItemData
            {
                id = "3asfkhjhjngnghds",
                owner = "",
                type = Enumerators.ItemType.Potion,
            });
            newItems.Add(new ItemData
            {
                id = "3asfkhjds",
                owner = "",
                type = Enumerators.ItemType.ScrollOfTeleport,
            });
            newItems.Add(new ItemData
            {
                id = "3askhjnghds",
                owner = "",
                type = Enumerators.ItemType.ScrollOfTeleport,
            });
            newItems.Add(new ItemData
            {
                id = "3asfkhghds",
                owner = "",
                type = Enumerators.ItemType.Potion,
            });
            
            _inventoryItems = newItems;
        }

        private ItemData ConvertToItemData(ResponseItemData responseItem)
        {
            return new ItemData
            {
                type = InternalTools.EnumFromString<Enumerators.ItemType>(responseItem.type),
                quality = InternalTools.EnumFromString<Enumerators.ItemQuality>(responseItem.quality),
                stats = responseItem.stats,
            };
        }

        public class ItemData
        {
            public string id;
            public string owner;
            public Enumerators.ItemType type;
            public Enumerators.ArtifactType ArtifactType;
            public Enumerators.ItemQuality quality;
            public ItemStats stats;
            
        }

        public enum ChangeType
        {
            Equip,
            Unequip
        }

        private class ItemChange
        {
            public ChangeType ChangeType { get; set; }
            public Inventory.ItemData Item { get; set; }
        }
    }
    
  

}