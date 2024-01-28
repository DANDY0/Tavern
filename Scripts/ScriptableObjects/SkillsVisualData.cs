using System;
using System.Collections.Generic;
using GrandDevs.Tavern.Common;
using UnityEngine;

namespace GrandDevs.Tavern
{
    [CreateAssetMenu(fileName = "SkillsVisualData", menuName = "GrandDevs/Tavern/SkillsVisualData", order = 4)]
    public class SkillsVisualData : ScriptableObject
    {
        public List<CharacterVisualData> CharacterVisualDatas;
        public List<SkillData> SkillsData;
        public List<Sprite> MovePatterns;
    }
    
    [Serializable]
    public class CharacterVisualData
    {
        public Sprite Avatar;
        public UltimateData UltimateData;
    }
    
    [Serializable]
    public class UltimateData
    {
        public Enumerators.CellType CellType;
        public Sprite UltimateIcon;
        public string Name;
        public string Description;
    }
    
    [Serializable]
    public class SkillData
    {
        public Enumerators.SkillNames CellType;
        public Sprite SkillIcon;
        public string Name;
        public string Description;
    }
}