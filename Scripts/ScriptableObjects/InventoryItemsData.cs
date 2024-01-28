using System;
using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    [CreateAssetMenu(fileName = "InventoryItemsData", menuName = "GrandDevs/Tavern/InventoryItemsData", order = 4)]
    public class InventoryItemsData : ScriptableObject
    {
        public List<ArtefactItemData> ArtefactItemsData;
        public List<MainItemData> MainItemsData;
    }

    public abstract class ItemVisualDataBase
    {
        public VisualData VisualData;
    }

    [Serializable]
    public class ArtefactItemData: ItemVisualDataBase
    {
        public Enumerators.ArtifactType Type;
    }

    [Serializable]
    public class MainItemData: ItemVisualDataBase
    {
        public Enumerators.ItemType Types;
    }

    
    [Serializable]
    public class VisualData
    {
        public Sprite ItemSprite;
        public string Description;
    }
}