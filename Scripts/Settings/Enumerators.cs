namespace GrandDevs.Tavern.Common
{
    public class Enumerators
    {
        public enum AppState
        {
            Unknown,

            AppStart,
            Main,
            Game
        }

        public enum InputType
        {
            Unknown,

            Mouse,
            Keyboard,
            Swipe
        }

        public enum SoundType
        {
            Unknown,

            Click
        }

        public enum Language
        {
            Unknown,

            English,
            German
        }

        public enum GameDataType
        {
            Unknown,

            UserData
        }

        public enum Direction
        {
            Unknown,

            Left,
            Right,
            Up,
            Down
        }

        public enum SpreadsheetDataType
        {
            Localization
        }

        public enum CellType
        {
            AttackRegular,
            AttackRange,
            Heal,
            Defense,
            Action,
            Empty
        }

        public enum SkillNames
        {
            FirstSkill,
            SecondSkill,
            ThirdSkill
        }
        
        public enum UnitType
        {
            Empty,
            Character,
            Bonus,
        }

        public enum ActionType
        {
            Unknown,
            MoveSelf,
            AttackHorizontal,
            AttackVertical,
            AttackAround,
            HealSelf,
            DefenseSelf,
            AttackRegular,
            AttackRange,
            NuclearBomb,
            DefenseEqualHealthSelf,
            HealthIsHalfToOther,
            FreezeMoveForOther,
        }
        
        public enum MovePattern
        {
            KingMove,
            RookMove,
            BishopMove,
            QuinMove,
        }
        
        public enum RoundState
        {
            RoundStart,
            RoundProcessing,
            RoundEnd,
            None
        }

        public enum AttackType
        {
            Melee,
            Range,
        }
        
        public enum ItemType
        {
            Character,
            Artifact,
            Potion,
            ScrollOfTeleport
        }
        
        public enum FilterButtonTypes
        {
            All,
            Potion,
            ScrollOfTeleport,
        }

        public enum ArtifactType
        {
            Helmet,
            Hands,
            Weapon,
            Legs,
            Shield,
            Necklace
        }

        public enum ItemQuality
        {
            Free,
            Common,
            Rare,
            Epic,
            Legendary
        }

        public enum MapLocationType
        {
            Tavern,
            ThievesCamp,
            City,
            Dungeon
        }
        
        public enum MapActions
        {
            Battle,
            Item,
            Quest
        }
    }
}