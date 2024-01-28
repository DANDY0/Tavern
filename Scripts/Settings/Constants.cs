using UnityEngine;

namespace GrandDevs.Tavern
{
    public class Constants
    {
        public const bool DataEncrypted = false;
        public const string CellTag = "Cell";
        public const int MaxPlayersCount = 4;
        
        public const string Url = "https://external.frostweepgames.com:8101/";
        
        //PlayerPrefsKeys
        public const string PlayerNameKey = "PlayerName";
        public const string ChosenCharacterKey = "ChosenCharacter";        

        public const string DefaultAttackVfxPath = "Prefabs/VFX/AttackDefaultVfx";
        public const string MissVfxPath = "Prefabs/VFX/MissVfx";
        public const string FireballVfxPath = "Prefabs/VFX/FireballVfx";
        public const string ExplosionFireballVfxPath = "Prefabs/VFX/ExplosionFireballVfx";
        public const string DefenceVfxPath = "Prefabs/VFX/DefenseVfx";
        public const string HealVfxPath = "Prefabs/VFX/HealVfx";
        public const string FreezeVfxPath = "Prefabs/VFX/FreezeVfx";
        public const string DefenseEqualHealthPath = "Prefabs/VFX/DefenseEqualHealthVfx";
        public const string HealthHalfEnemiesPath = "Prefabs/VFX/HealthHalfEnemiesVfx";
        public const string TurnVfxPath = "Prefabs/VFX/TurnVfx";

        public static readonly Vector3 CellOffset = new Vector3(-1, 0, 1);
        public static readonly Vector3 CameraOffset = new Vector3(0, 7, -3.5f);

        public static readonly int IdleHash = Animator.StringToHash("IDLE");
        public static readonly int MoveHash = Animator.StringToHash("MOVE");
        public static readonly int AttackHorizontalHash = Animator.StringToHash("ULT");
        public static readonly int AttackVerticalHash = Animator.StringToHash("ULT");
        public static readonly int AttackAroundHash = Animator.StringToHash("ATTACK");
        public static readonly int DefenseHash = Animator.StringToHash("DEFENSE");
        public static readonly int HealHash = Animator.StringToHash("HEAL");
        public static readonly int TakeSoftDamageHash = Animator.StringToHash("GET_HIT");
        public static readonly int DeadHash = Animator.StringToHash("DEAD");
        public static readonly int DeadBoolHash = Animator.StringToHash("IS_DEAD");

    }
}