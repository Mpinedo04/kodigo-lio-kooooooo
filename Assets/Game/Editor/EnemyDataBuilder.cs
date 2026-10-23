using System.Collections.Generic;
using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // F06 data: Season 1 monster values (Biblia 8.1), wave assets and the combat director profile.
    public static class EnemyDataBuilder
    {
        private const string DataFolder = "Assets/Game/Data";
        private const string EnemyFolder = DataFolder + "/Enemies";
        private const string WaveFolder = DataFolder + "/Waves";
        private const string DirectorProfilePath = DataFolder + "/CombatDirectorProfile.asset";

        public static readonly EnemyKind[] SeasonOneKinds =
        {
            EnemyKind.Kankrelat, EnemyKind.Blok, EnemyKind.Krab, EnemyKind.Hornet, EnemyKind.Megatank
        };

        [MenuItem("Tools/Code Lyoko Fan Game/Create Enemy Data (F06)")]
        public static void CreateEnemyData()
        {
            ApplySeasonOne(SeasonOne());
            EnsureDirectorProfile();
            EnsureForestWaves();
            EnsureMixedGroup();
            EnsureKankrelatGroup();
            AssetDatabase.SaveAssets();
            Debug.Log("F06 enemy data created in " + DataFolder);
        }

        // Writes the canon values and attack lists into the Season 1 assets.
        public static void ApplySeasonOne(params EnemyDefinition[] definitions)
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] == null)
                {
                    continue;
                }

                EnemyDefaults.ApplySeasonOne(definitions[i]);
                EditorUtility.SetDirty(definitions[i]);
            }
        }

        public static EnemyDefinition[] SeasonOne()
        {
            EnemyDefinition[] result = new EnemyDefinition[SeasonOneKinds.Length];
            for (int i = 0; i < SeasonOneKinds.Length; i++)
            {
                result[i] = Find(SeasonOneKinds[i]);
            }

            return result;
        }

        public static EnemyDefinition Find(EnemyKind kind)
        {
            EnemyDefinition named = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(EnemyFolder + "/" + kind + ".asset");
            if (named != null && named.kind == kind)
            {
                return named;
            }

            if (!AssetDatabase.IsValidFolder(EnemyFolder))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { EnemyFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (definition != null && definition.kind == kind)
                {
                    return definition;
                }
            }

            return null;
        }

        // Created once; later rebuilds keep the values tuned in the Inspector.
        public static CombatDirectorProfile EnsureDirectorProfile()
        {
            CombatDirectorProfile profile = AssetDatabase.LoadAssetAtPath<CombatDirectorProfile>(DirectorProfilePath);
            if (profile == null)
            {
                Directory.CreateDirectory(DataFolder);
                profile = ScriptableObject.CreateInstance<CombatDirectorProfile>();
                AssetDatabase.CreateAsset(profile, DirectorProfilePath);
            }

            return profile;
        }

        // Forest tower reinforcements: three Kankrelats, then two Hornets once they are down.
        public static List<WaveDefinition> EnsureForestWaves()
        {
            return new List<WaveDefinition>
            {
                Wave("Wave_ForestTower_1", "Refuerzos: Kankrelats", 0, 2f, Group(EnemyKind.Kankrelat, 3, 4f, 0.35f)),
                Wave("Wave_ForestTower_2", "Refuerzos: Hornets", 0, 2f, Group(EnemyKind.Hornet, 2, 6f, 0.5f))
            };
        }

        public static WaveDefinition EnsureMixedGroup()
        {
            return Wave("Wave_EnemyGym_Mixed", "Grupo mixto", 0, 2f,
                Group(EnemyKind.Kankrelat, 3, 5f, 0.2f),
                Group(EnemyKind.Blok, 1, 3f, 0.2f),
                Group(EnemyKind.Hornet, 1, 4f, 0.2f));
        }

        public static WaveDefinition EnsureKankrelatGroup()
        {
            return Wave("Wave_EnemyGym_Kankrelats", "Grupo de Kankrelats", 0, 2f, Group(EnemyKind.Kankrelat, 5, 4f, 0.2f));
        }

        private static WaveDefinition Wave(string assetName, string label, int threshold, float delay, params WaveGroup[] groups)
        {
            Directory.CreateDirectory(WaveFolder);
            string path = WaveFolder + "/" + assetName + ".asset";
            WaveDefinition wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
            if (wave == null)
            {
                wave = ScriptableObject.CreateInstance<WaveDefinition>();
                AssetDatabase.CreateAsset(wave, path);
            }

            wave.waveName = label;
            wave.groups = new List<WaveGroup>(groups);
            wave.nextWaveWhenAliveAtMost = threshold;
            wave.delayBeforeNextWave = delay;
            EditorUtility.SetDirty(wave);
            return wave;
        }

        private static WaveGroup Group(EnemyKind kind, int count, float radius, float delay)
        {
            return new WaveGroup
            {
                definition = Find(kind),
                fallbackKind = kind,
                count = count,
                radius = radius,
                delayBetweenSpawns = delay
            };
        }
    }
}
