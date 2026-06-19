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
            CharacterDefinition jeremie = Character("Jeremie", LyokoCharacter.Jeremie, AttackStyle.Operator, VehicleStyle.None, 90f, 5.8f, 7.2f, 4f, 10f, false, "Operator character. Runs mission support, scanners and tower guidance from the lab.");
            jeremie.operatorOnly = true;
            CharacterDefinition william = Character("William", LyokoCharacter.William, AttackStyle.Melee, VehicleStyle.BlackManta, 160f, 7.1f, 10.2f, 28f, 48f, false, "Heavy sword fighter. Can become playable after story progression.");
            CharacterDefinition xanaWilliam = Character("XanaWilliam", LyokoCharacter.XanaWilliam, AttackStyle.Melee, VehicleStyle.BlackManta, 280f, 7.4f, 10.8f, 34f, 58f, false, "X.A.N.A.-controlled William rival/boss form.");

            EnemyDefinition kankrelat = Enemy("Kankrelat", EnemyKind.Kankrelat, EnemyBehaviorStyle.SwarmShooter, 60f, 22f, 15f, 3.8f, 9f, 0.95f, false, false, true, "Weak swarmer. Small and fast, best used in groups.");
            EnemyDefinition blok = Enemy("Blok", EnemyKind.Blok, EnemyBehaviorStyle.Turret, 85f, 26f, 20f, 1.8f, 14f, 1f, false, false, true, "Cube turret with room for laser, fire ring and freeze attacks.");
            EnemyDefinition krab = Enemy("Krab", EnemyKind.Krab, EnemyBehaviorStyle.HeavyWalker, 120f, 28f, 18f, 3.2f, 18f, 1.15f, false, false, true, "Tall heavy monster with leg attacks and strong eye lasers.");
            EnemyDefinition hornet = Enemy("Hornet", EnemyKind.Hornet, EnemyBehaviorStyle.FlyingShooter, 55f, 30f, 22f, 4.8f, 12f, 0.72f, true, false, true, "Flying rapid shooter. Should strafe around the player.");
            EnemyDefinition megatank = Enemy("Megatank", EnemyKind.Megatank, EnemyBehaviorStyle.ArmoredBurst, 260f, 34f, 24f, 1.4f, 34f, 2.4f, false, true, true, "Armored boss. Exposes weak point during heavy laser wind-up.");
            megatank.weakPointAlwaysExposed = false;
            EnemyDefinition tarantula = Enemy("Tarantula", EnemyKind.Tarantula, EnemyBehaviorStyle.EliteShooter, 150f, 32f, 25f, 2.8f, 22f, 0.8f, false, false, false, "Durable elite shooter with high-damage front-leg lasers.");
            EnemyDefinition creeper = Enemy("Creeper", EnemyKind.Creeper, EnemyBehaviorStyle.Ambusher, 75f, 22f, 12f, 5.2f, 18f, 1f, false, false, false, "Sector 5 snake-like ambusher. Dangerous in close range.");
            creeper.contactDamage = 32f;
            EnemyDefinition manta = Enemy("Manta", EnemyKind.Manta, EnemyBehaviorStyle.MineLayer, 120f, 34f, 25f, 4.2f, 18f, 1.1f, true, false, false, "Heavy flying monster that should eventually lay mines.");
            manta.hoverHeight = 5.2f;
            EnemyDefinition blackManta = Enemy("BlackManta", EnemyKind.BlackManta, EnemyBehaviorStyle.MineLayer, 180f, 36f, 26f, 4.8f, 22f, 0.9f, true, false, false, "William's X.A.N.A. manta vehicle/support monster.");
            blackManta.hoverHeight = 5.4f;
            EnemyDefinition kongre = Enemy("Kongre", EnemyKind.Kongre, EnemyBehaviorStyle.DigitalSeaChaser, 90f, 34f, 18f, 5f, 16f, 1.1f, true, false, false, "Digital Sea eel/piranha chaser.");
            kongre.digitalSeaOnly = true;
            EnemyDefinition shark = Enemy("Shark", EnemyKind.Shark, EnemyBehaviorStyle.DigitalSeaChaser, 120f, 34f, 18f, 5.4f, 24f, 1.3f, true, false, false, "Digital Sea shark enemy with torpedo-style attacks.");
            shark.digitalSeaOnly = true;
            EnemyDefinition kalamar = Enemy("Kalamar", EnemyKind.Kalamar, EnemyBehaviorStyle.Grappler, 190f, 30f, 10f, 3.4f, 26f, 1.8f, true, false, false, "Skidbladnir grappler/driller monster.");
            kalamar.digitalSeaOnly = true;
            kalamar.contactDamage = 42f;
            EnemyDefinition guardian = Enemy("Guardian", EnemyKind.Guardian, EnemyBehaviorStyle.CaptureField, 220f, 20f, 6f, 2.2f, 12f, 2f, false, false, false, "Capture-field monster/phenomenon. Should become puzzle-based.");
            guardian.weakPointAlwaysExposed = false;
            EnemyDefinition scyphozoa = Enemy("Scyphozoa", EnemyKind.Scyphozoa, EnemyBehaviorStyle.MemoryDrainer, 260f, 30f, 5f, 2.6f, 20f, 1f, true, false, false, "Memory-draining jellyfish monster used for possession and source-code theft.");
            scyphozoa.hoverHeight = 4.2f;
            scyphozoa.weakPointAlwaysExposed = false;
            EnemyDefinition kolossus = Enemy("Kolossus", EnemyKind.Kolossus, EnemyBehaviorStyle.RaidBoss, 900f, 42f, 30f, 1.1f, 60f, 3.4f, false, true, false, "Final raid boss. Huge lava body, sword arm, multi-weak-point fight.");
            kolossus.weakPointAlwaysExposed = false;
            EnemyDefinition specter = Enemy("Specter", EnemyKind.Specter, EnemyBehaviorStyle.RealWorldPossessor, 80f, 24f, 8f, 3.6f, 10f, 1.2f, false, false, false, "Real-world X.A.N.A. manifestation.");
            EnemyDefinition polymorphicClone = Enemy("PolymorphicClone", EnemyKind.PolymorphicClone, EnemyBehaviorStyle.RealWorldPossessor, 100f, 26f, 10f, 3.8f, 14f, 1.1f, false, false, false, "Disguise/clone enemy for real-world missions.");
            EnemyDefinition zombies = Enemy("Zombies", EnemyKind.Zombies, EnemyBehaviorStyle.RealWorldPossessor, 70f, 18f, 4f, 2.8f, 8f, 1.5f, false, false, false, "Real-world swarm event enemy.");
            EnemyDefinition xanaWilliamEnemy = Enemy("XanaWilliamEnemy", EnemyKind.XanaWilliam, EnemyBehaviorStyle.Duelist, 280f, 34f, 18f, 4.2f, 28f, 1f, false, true, false, "Rival duelist boss with sword and Black Manta support.");
            xanaWilliamEnemy.contactDamage = 45f;

            VehicleDefinition overboard = Vehicle("Overboard", VehicleStyle.Overboard, LyokoCharacter.Odd, 1.7f, true, false, true, "Odd's purple flying board. Highest agility.");
            VehicleDefinition overbike = Vehicle("Overbike", VehicleStyle.Overbike, LyokoCharacter.Ulrich, 1.85f, true, false, true, "Ulrich's green one-wheel bike with flight capability.");
            VehicleDefinition overwing = Vehicle("Overwing", VehicleStyle.Overwing, LyokoCharacter.Yumi, 1.45f, true, true, true, "Yumi's stable hover-scooter. Best passenger vehicle.");
            VehicleDefinition skid = Vehicle("Skidbladnir", VehicleStyle.Skidbladnir, LyokoCharacter.Aelita, 1.25f, true, true, false, "Team submarine for the Digital Sea.");
            skid.digitalSeaVehicle = true;
            VehicleDefinition navSkid = Vehicle("NavSkid", VehicleStyle.NavSkid, LyokoCharacter.Odd, 1.6f, true, false, false, "Individual Digital Sea pod launched from the Skidbladnir.");
            navSkid.digitalSeaVehicle = true;
            VehicleDefinition transportOrb = Vehicle("TransportOrb", VehicleStyle.TransportOrb, LyokoCharacter.Aelita, 1.2f, true, true, false, "Orb used for team/sector transport.");
            VehicleDefinition blackMantaVehicle = Vehicle("BlackMantaVehicle", VehicleStyle.BlackManta, LyokoCharacter.XanaWilliam, 1.75f, true, true, false, "X.A.N.A. William's dark flying mount.");

            RosterDatabase roster = LoadOrCreate<RosterDatabase>("RosterDatabase");
            roster.characters.Clear();
            roster.characters.AddRange(new[] { odd, ulrich, yumi, aelita, jeremie, william, xanaWilliam });
            roster.enemies.Clear();
            roster.enemies.AddRange(new[]
            {
                kankrelat, blok, krab, hornet, megatank, tarantula, creeper, manta, blackManta,
                kongre, shark, kalamar, guardian, scyphozoa, kolossus, specter, polymorphicClone,
                zombies, xanaWilliamEnemy
            });
            roster.vehicles.Clear();
            roster.vehicles.AddRange(new[] { overboard, overbike, overwing, skid, navSkid, transportOrb, blackMantaVehicle });
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

        private static EnemyDefinition Enemy(string assetName, EnemyKind kind, EnemyBehaviorStyle behavior, float health, float detection, float attackRange, float speed, float damage, float cooldown, bool flying, bool boss, bool slice, string notes)
        {
            EnemyDefinition definition = LoadOrCreate<EnemyDefinition>("Enemies/" + assetName);
            definition.kind = kind;
            definition.behaviorStyle = behavior;
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
