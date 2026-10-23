using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Finds the EnemyDefinition asset of a monster kind at runtime (debug spawns, legacy waves).
    // Scene monsters, wave spawners and the gym panel register the assets they reference.
    public static class EnemyCatalog
    {
        private static readonly Dictionary<EnemyKind, EnemyDefinition> Known = new Dictionary<EnemyKind, EnemyDefinition>();

        public static void Register(EnemyDefinition definition)
        {
            if (definition == null || IsRuntimeFallback(definition))
            {
                return;
            }

            if (!Known.TryGetValue(definition.kind, out EnemyDefinition existing) || existing == null)
            {
                Known[definition.kind] = definition;
            }
        }

        public static EnemyDefinition Find(EnemyKind kind)
        {
            if (Known.TryGetValue(kind, out EnemyDefinition known) && known != null)
            {
                return known;
            }

            // Anything already loaded (referenced by the scene or the roster database).
            EnemyDefinition[] loaded = Resources.FindObjectsOfTypeAll<EnemyDefinition>();
            for (int i = 0; i < loaded.Length; i++)
            {
                if (loaded[i] != null && loaded[i].kind == kind && !IsRuntimeFallback(loaded[i]))
                {
                    Known[kind] = loaded[i];
                    return loaded[i];
                }
            }

            return null;
        }

        private static bool IsRuntimeFallback(EnemyDefinition definition)
        {
            return (definition.hideFlags & HideFlags.DontSave) != 0;
        }
    }
}
