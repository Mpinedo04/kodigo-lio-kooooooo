using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // Creates the combat physics layers and the collision rules between them.
    public static class PhysicsLayersSetup
    {
        private const int FirstUserLayer = 8;

        [MenuItem("Tools/Code Lyoko Fan Game/Setup Physics Layers")]
        public static void EnsureLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            foreach (string layerName in GameLayers.All)
            {
                if (HasLayer(layers, layerName))
                {
                    continue;
                }

                for (int i = FirstUserLayer; i < layers.arraySize; i++)
                {
                    SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(slot.stringValue))
                    {
                        slot.stringValue = layerName;
                        break;
                    }
                }
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();

            // Own shots never hit their own side; shots do not collide with each other.
            Ignore(GameLayers.PlayerAttack, GameLayers.Player);
            Ignore(GameLayers.PlayerAttack, GameLayers.PlayerAttack);
            Ignore(GameLayers.PlayerAttack, GameLayers.EnemyAttack);
            Ignore(GameLayers.EnemyAttack, GameLayers.Enemy);
            Ignore(GameLayers.EnemyAttack, GameLayers.EnemyAttack);
            AssetDatabase.SaveAssets();
        }

        private static bool HasLayer(SerializedProperty layers, string layerName)
        {
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Ignore(string a, string b)
        {
            int layerA = LayerMask.NameToLayer(a);
            int layerB = LayerMask.NameToLayer(b);
            if (layerA >= 0 && layerB >= 0)
            {
                Physics.IgnoreLayerCollision(layerA, layerB, true);
            }
        }
    }
}
