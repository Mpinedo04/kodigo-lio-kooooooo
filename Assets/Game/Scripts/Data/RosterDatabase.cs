using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame.Data
{
    [CreateAssetMenu(menuName = "Code Lyoko/Roster Database", fileName = "RosterDatabase")]
    public sealed class RosterDatabase : ScriptableObject
    {
        public List<CharacterDefinition> characters = new List<CharacterDefinition>();
        public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
        public List<VehicleDefinition> vehicles = new List<VehicleDefinition>();
    }
}
