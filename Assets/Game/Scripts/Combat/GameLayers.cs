using UnityEngine;

namespace CodeLyokoFanGame
{
    // Physics layer names (created by the editor builders) and safe lookups.
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string PlayerAttack = "PlayerAttack";
        public const string EnemyAttack = "EnemyAttack";
        public const string Environment = "Environment";
        public const string DigitalSea = "DigitalSea";

        public static readonly string[] All = { Player, Enemy, PlayerAttack, EnemyAttack, Environment, DigitalSea };

        // Layer index, or Default (0) when the layer does not exist yet.
        public static int Index(string layerName)
        {
            int index = LayerMask.NameToLayer(layerName);
            return index >= 0 ? index : 0;
        }

        public static bool Is(GameObject gameObject, string layerName)
        {
            int index = LayerMask.NameToLayer(layerName);
            return index >= 0 && gameObject.layer == index;
        }

        public static void SetRecursively(GameObject root, string layerName)
        {
            int index = LayerMask.NameToLayer(layerName);
            if (index < 0)
            {
                return;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = index;
            }
        }
    }
}
