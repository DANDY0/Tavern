using UnityEngine;

namespace GrandDevs.Tavern
{
    [CreateAssetMenu(fileName = "GameplayData", menuName = "GrandDevs/Tavern/GameplayData", order = 3)]
    public class GameplayData : ScriptableObject
    {
        [Range(30, 240)]
        public int targetFrameRate = 60;
    }
}