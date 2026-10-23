using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // F05 data: how each ability hits (damage, radius, clones...) and the warriors' attack details.
    // Called by PrototypeDataAssetBuilder; the values follow Biblia section 5.
    public static class AbilityDataBuilder
    {
        private const string SquadProfilePath = "Assets/Game/Data/SquadProfile.asset";

        public static void TuneAll(params AbilityDefinition[] abilities)
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                Tune(abilities[i]);
                EditorUtility.SetDirty(abilities[i]);
            }
        }

        private static void Tune(AbilityDefinition a)
        {
            switch (a.abilityId)
            {
                case "odd_shield":
                    a.abilityHealth = 60f;
                    break;
                case "odd_future_flash":
                    a.timeScale = 0.35f;
                    break;
                case "odd_feline_rain":
                    Set(a, damage: 18f, radius: 6f, speed: 14f, count: 18);
                    a.duration = 1.5f;
                    break;
                case "ulrich_supersprint":
                    Set(a, damage: 25f, radius: 1.4f, range: 14f, speed: 28f);
                    a.duration = 0.45f;
                    break;
                case "ulrich_triplicate":
                    Set(a, damage: 5f, range: 16f, speed: 8f, count: 2, abilityHealth: 1f);
                    a.power = 3f;
                    break;
                case "ulrich_triangulate":
                    a.power = 3f;
                    break;
                case "ulrich_impact":
                    Set(a, damage: 60f, radius: 6f);
                    a.power = 14f;
                    break;
                case "ulrich_katana_deflect":
                    a.power = 1.5f;
                    break;
                case "yumi_tk_grab":
                    Set(a, damage: 35f, range: 14f, speed: 26f, costPerSecond: 12f);
                    break;
                case "yumi_tk_wall":
                    Set(a, radius: 4.5f, range: 3f);
                    a.power = 3f;
                    break;
                case "yumi_fan_dance":
                    Set(a, damage: 6f, radius: 7f);
                    break;
                case "aelita_creativity_bridge":
                case "aelita_creativity_wall":
                    a.range = 8f;
                    break;
                case "aelita_wings":
                    Set(a, speed: 2.5f, costPerSecond: 12f);
                    a.cost = 25f;
                    a.power = 7f;
                    break;
            }
        }

        private static void Set(AbilityDefinition a, float damage = 0f, float radius = 0f, float range = 0f, float speed = 0f, int count = 0, float abilityHealth = 0f, float costPerSecond = 0f)
        {
            a.damage = damage;
            a.radius = radius;
            a.range = range;
            a.speed = speed;
            a.count = count;
            a.abilityHealth = abilityHealth;
            a.costPerSecond = costPerSecond;
        }

        // Odd fires arrows in bursts and his charged arrow stuns; Aelita's energy fields are slow, homing and burst in an area.
        public static void ApplyAttackDetails(CharacterDefinition odd, CharacterDefinition aelita)
        {
            odd.basicBurst = 2;
            odd.specialStagger = 1.2f;
            EditorUtility.SetDirty(odd);

            aelita.attackStyle = AttackStyle.Support;
            aelita.basicHoming = 3f;
            aelita.basicProjectileSpeed = 0.55f;
            aelita.specialExplosionRadius = 4f;
            EditorUtility.SetDirty(aelita);
        }

        // Created once; later rebuilds keep the values tuned in the Inspector.
        public static SquadProfile EnsureSquadProfile()
        {
            SquadProfile profile = AssetDatabase.LoadAssetAtPath<SquadProfile>(SquadProfilePath);
            if (profile == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SquadProfilePath));
                profile = ScriptableObject.CreateInstance<SquadProfile>();
                AssetDatabase.CreateAsset(profile, SquadProfilePath);
            }

            return profile;
        }
    }
}
