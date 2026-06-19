using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    public static class PrototypeDataAssetBuilder
    {
        private const string DataFolder = "Assets/Game/Data";

        [MenuItem("Tools/Code Lyoko Fan Game/Create Prototype Data Assets")]
        public static void CreatePrototypeDataAssets()
        {
            Directory.CreateDirectory(DataFolder);

            CharacterDefinition odd = Character("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, 120f, 7.8f, 11f, 18f, 32f, true, "Fast ranged fighter. Uses laser arrows and the Overboard.");
            CharacterDefinition ulrich = Character("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, 140f, 7.2f, 10.5f, 24f, 42f, true, "Close-range katana fighter. Uses speed, dash and the Overbike.");
            CharacterDefinition yumi = Character("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, 130f, 7f, 10f, 20f, 38f, true, "Balanced fighter with tessen fans and light telekinesis. Uses the Overwing.");
            CharacterDefinition aelita = Character("Aelita", LyokoCharacter.Aelita, AttackStyle.Ranged, VehicleStyle.Overwing, 100f, 6.6f, 9.2f, 12f, 24f, false, "Tower key character. Uses creativity, shields and Code: LYOKO interactions.");

            EnemyDefinition kankrelat = Enemy("Kankrelat", EnemyKind.Kankrelat, 60f, 22f, 15f, 3.8f, 9f, 0.95f, false, false, true, "Weak swarmer. Small and fast, best used in groups.");
            EnemyDefinition blok = Enemy("Blok", EnemyKind.Blok, 85f, 26f, 20f, 1.8f, 14f, 1f, false, false, true, "Cube turret with room for laser, fire ring and freeze attacks.");
            EnemyDefinition krab = Enemy("Krab", EnemyKind.Krab, 120f, 28f, 18f, 3.2f, 18f, 1.15f, false, false, true, "Tall heavy monster with leg attacks and strong eye lasers.");
            EnemyDefinition hornet = Enemy("Hornet", EnemyKind.Hornet, 55f, 30f, 22f, 4.8f, 12f, 0.72f, true, false, true, "Flying rapid shooter. Should strafe around the player.");
            EnemyDefinition megatank = Enemy("Megatank", EnemyKind.Megatank, 260f, 34f, 24f, 1.4f, 34f, 2.4f, false, true, true, "Armored boss. Exposes weak point during heavy laser wind-up.");

            VehicleDefinition overboard = Vehicle("Overboard", VehicleStyle.Overboard, LyokoCharacter.Odd, 1.7f, true, false, true, "Odd's purple flying board. Highest agility.");
            VehicleDefinition overbike = Vehicle("Overbike", VehicleStyle.Overbike, LyokoCharacter.Ulrich, 1.85f, true, false, true, "Ulrich's green one-wheel bike with flight capability.");
            VehicleDefinition overwing = Vehicle("Overwing", VehicleStyle.Overwing, LyokoCharacter.Yumi, 1.45f, true, true, true, "Yumi's stable hover-scooter. Best passenger vehicle.");

            RosterDatabase roster = LoadOrCreate<RosterDatabase>("RosterDatabase");
            roster.characters.Clear();
            roster.characters.AddRange(new[] { odd, ulrich, yumi, aelita });
            roster.enemies.Clear();
            roster.enemies.AddRange(new[] { kankrelat, blok, krab, hornet, megatank });
            roster.vehicles.Clear();
            roster.vehicles.AddRange(new[] { overboard, overbike, overwing });
            EditorUtility.SetDirty(roster);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Prototype data assets created in " + DataFolder);
        }

        private static CharacterDefinition Character(string assetName, LyokoCharacter id, AttackStyle attack, VehicleStyle vehicle, float health, float walk, float sprint, float basic, float special, bool playable, string notes)
        {
            CharacterDefinition definition = LoadOrCreate<CharacterDefinition>("Characters/" + assetName);
            definition.character = id;
            definition.displayName = assetName;
            definition.attackStyle = attack;
            definition.vehicleStyle = vehicle;
            definition.maxHealth = health;
            definition.walkSpeed = walk;
            definition.sprintSpeed = sprint;
            definition.basicDamage = basic;
            definition.specialDamage = special;
            definition.playableInVerticalSlice = playable;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static EnemyDefinition Enemy(string assetName, EnemyKind kind, float health, float detection, float attackRange, float speed, float damage, float cooldown, bool flying, bool boss, bool slice, string notes)
        {
            EnemyDefinition definition = LoadOrCreate<EnemyDefinition>("Enemies/" + assetName);
            definition.kind = kind;
            definition.displayName = assetName;
            definition.maxHealth = health;
            definition.detectionRange = detection;
            definition.attackRange = attackRange;
            definition.moveSpeed = speed;
            definition.projectileDamage = damage;
            definition.fireCooldown = cooldown;
            definition.flying = flying;
            definition.boss = boss;
            definition.appearsInVerticalSlice = slice;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static VehicleDefinition Vehicle(string assetName, VehicleStyle style, LyokoCharacter rider, float speedMultiplier, bool canFly, bool passenger, bool slice, string notes)
        {
            VehicleDefinition definition = LoadOrCreate<VehicleDefinition>("Vehicles/" + assetName);
            definition.style = style;
            definition.displayName = assetName;
            definition.defaultRider = rider;
            definition.speedMultiplier = speedMultiplier;
            definition.canFly = canFly;
            definition.canCarryPassenger = passenger;
            definition.appearsInVerticalSlice = slice;
            definition.canonNotes = notes;
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static T LoadOrCreate<T>(string relativePath) where T : ScriptableObject
        {
            string path = DataFolder + "/" + relativePath + ".asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
