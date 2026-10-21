using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    // Test scene for combat (F04): combos, charged attacks, dodge, parry, lock-on and devirtualization
    // against two Kankrelats, a Krab and a Blok turret. Not part of the build.
    public static class CombatArenaBuilder
    {
        public const string ScenePath = "Assets/Scenes/Test/CombatArena.unity";

        [MenuItem("Tools/Code Lyoko Fan Game/Build Combat Arena Scene")]
        public static void BuildCombatArena()
        {
            CodeLyokoSceneBuilder.EnsureFolders();
            UrpPipelineSetup.EnsurePipeline();
            CodeLyokoSceneBuilder.EnsureTag("Player");
            CodeLyokoSceneBuilder.EnsureTag("Enemy");
            PhysicsLayersSetup.EnsureLayers();
            InputReader input = CoreAssetsBuilder.EnsureInputReader();

            Material floor = CodeLyokoSceneBuilder.Material("Gym_Floor", new Color(0.32f, 0.38f, 0.4f), 0f);
            Material wall = CodeLyokoSceneBuilder.Material("Lyoko_Dark_Metal", new Color(0.06f, 0.08f, 0.09f), 0.1f);
            Material cyan = CodeLyokoSceneBuilder.Material("Lyoko_Cyan_Emission", new Color(0.1f, 0.85f, 1f), 1.7f);
            Material red = CodeLyokoSceneBuilder.Material("Xana_Red_Emission", new Color(1f, 0.06f, 0.04f), 2.2f);
            Material orange = CodeLyokoSceneBuilder.Material("Xana_Orange", new Color(1f, 0.38f, 0.03f), 1.4f);
            Material violet = CodeLyokoSceneBuilder.Material("Odd_Violet", new Color(0.45f, 0.18f, 0.95f), 0.4f);
            Material green = CodeLyokoSceneBuilder.Material("Ulrich_Green", new Color(0.06f, 0.42f, 0.2f), 0.3f);
            Material black = CodeLyokoSceneBuilder.Material("Yumi_Black", new Color(0.02f, 0.02f, 0.025f), 0.2f);

            Projectile playerProjectile = CodeLyokoSceneBuilder.BuildProjectilePrefab("PlayerEnergyDart", cyan, false);
            Projectile enemyProjectile = CodeLyokoSceneBuilder.BuildProjectilePrefab("XanaLaser", red, true);
            VehicleRig overboard = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overboard", VehicleStyle.Overboard, violet, PrimitiveType.Cube);
            VehicleRig overbike = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overbike", VehicleStyle.Overbike, green, PrimitiveType.Cylinder);
            VehicleRig overwing = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overwing", VehicleStyle.Overwing, black, PrimitiveType.Cube);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = null;
            RenderSettings.ambientLight = new Color(0.45f, 0.52f, 0.55f);
            CodeLyokoSceneBuilder.CreateLighting();

            GameObject systems = new GameObject("GameSystems");
            LockOnSystem lockOn = systems.AddComponent<LockOnSystem>();
            CharacterSwitcher switcher = systems.AddComponent<CharacterSwitcher>();
            GameHud hud = systems.AddComponent<GameHud>();
            DigitalSeaHazard sea = systems.AddComponent<DigitalSeaHazard>();

            CameraRig cameraRig = CodeLyokoSceneBuilder.CreateCameraRig(lockOn, input);
            CodeLyokoSceneBuilder.SetPrivate(switcher, "cameraRig", cameraRig);
            CodeLyokoSceneBuilder.SetPrivate(switcher, "lockOnSystem", lockOn);
            CodeLyokoSceneBuilder.SetPrivate(switcher, "input", input);
            CodeLyokoSceneBuilder.SetPrivate(lockOn, "input", input);

            Transform respawn = new GameObject("RespawnPoint").transform;
            respawn.position = new Vector3(0f, 2f, -14f);
            sea.Configure(true, respawn);

            // Closed arena, 40 x 40, top of the floor at y = 0.
            CodeLyokoSceneBuilder.CreatePlatform("Arena_Floor", new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f), floor);
            CodeLyokoSceneBuilder.CreatePlatform("Arena_Wall_N", new Vector3(0f, 1.5f, 20.5f), new Vector3(42f, 3f, 1f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Arena_Wall_S", new Vector3(0f, 1.5f, -20.5f), new Vector3(42f, 3f, 1f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Arena_Wall_E", new Vector3(20.5f, 1.5f, 0f), new Vector3(1f, 3f, 42f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Arena_Wall_W", new Vector3(-20.5f, 1.5f, 0f), new Vector3(1f, 3f, 42f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Cover_A", new Vector3(-5f, 1f, 0f), new Vector3(3f, 2f, 1f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Cover_B", new Vector3(5f, 1f, 4f), new Vector3(3f, 2f, 1f), wall);

            Vector3 spawn = new Vector3(0f, 1f, -14f);
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, spawn));
            hud.Configure(switcher, null, null, null);

            CodeLyokoSceneBuilder.CreateEnemy("Kankrelat_A", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(-8f, 0.5f, 8f));
            CodeLyokoSceneBuilder.CreateEnemy("Kankrelat_B", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(8f, 0.5f, 8f));
            CodeLyokoSceneBuilder.CreateEnemy("Krab_A", EnemyKind.Krab, wall, red, enemyProjectile, new Vector3(0f, 0.5f, 12f));
            CodeLyokoSceneBuilder.CreateEnemy("Blok_ParryPractice", EnemyKind.Blok, wall, red, enemyProjectile, new Vector3(0f, 0.5f, 18f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Combat arena generated at " + ScenePath);
        }
    }
}
