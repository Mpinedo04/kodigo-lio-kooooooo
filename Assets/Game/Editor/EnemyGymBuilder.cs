using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    // Test scene for the Season 1 monsters (F06): flat arena with a NavMesh, a ramp up to a plateau,
    // cover, an open south edge over the Digital Sea, and a debug panel to spawn each monster and
    // mixed groups. Not part of the build.
    public static class EnemyGymBuilder
    {
        public const string ScenePath = "Assets/Scenes/Test/EnemyGym.unity";

        [MenuItem("Tools/Code Lyoko Fan Game/Build Enemy Gym Scene")]
        public static void BuildEnemyGym()
        {
            CodeLyokoSceneBuilder.EnsureFolders();
            UrpPipelineSetup.EnsurePipeline();
            CodeLyokoSceneBuilder.EnsureTag("Player");
            CodeLyokoSceneBuilder.EnsureTag("Enemy");
            PhysicsLayersSetup.EnsureLayers();
            InputReader input = CoreAssetsBuilder.EnsureInputReader();

            Material floor = CodeLyokoSceneBuilder.Material("Gym_Floor", new Color(0.32f, 0.38f, 0.4f), 0f);
            Material accent = CodeLyokoSceneBuilder.Material("Lyoko_Forest_Green", new Color(0.08f, 0.7f, 0.33f), 0.2f);
            Material wall = CodeLyokoSceneBuilder.Material("Lyoko_Dark_Metal", new Color(0.06f, 0.08f, 0.09f), 0.1f);
            Material cyan = CodeLyokoSceneBuilder.Material("Lyoko_Cyan_Emission", new Color(0.1f, 0.85f, 1f), 1.7f);
            Material red = CodeLyokoSceneBuilder.Material("Xana_Red_Emission", new Color(1f, 0.06f, 0.04f), 2.2f);
            Material violet = CodeLyokoSceneBuilder.Material("Odd_Violet", new Color(0.45f, 0.18f, 0.95f), 0.4f);
            Material green = CodeLyokoSceneBuilder.Material("Ulrich_Green", new Color(0.06f, 0.42f, 0.2f), 0.3f);
            Material black = CodeLyokoSceneBuilder.Material("Yumi_Black", new Color(0.02f, 0.02f, 0.025f), 0.2f);

            Projectile playerProjectile = CodeLyokoSceneBuilder.BuildProjectilePrefab("PlayerEnergyDart", cyan, false);
            Projectile enemyProjectile = CodeLyokoSceneBuilder.BuildProjectilePrefab("XanaLaser", red, true);
            VehicleController overboard = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overboard", VehicleStyle.Overboard, violet, PrimitiveType.Cube);
            VehicleController overbike = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overbike", VehicleStyle.Overbike, green, PrimitiveType.Cylinder);
            VehicleController overwing = CodeLyokoSceneBuilder.BuildVehiclePrefab("Overwing", VehicleStyle.Overwing, black, PrimitiveType.Cube);

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

            BuildArena(floor, accent, wall, cyan);

            Vector3 spawn = new Vector3(0f, 1f, -14f);
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, spawn));
            hud.Configure(switcher, null, null, null);

            Transform spawnCenter = new GameObject("MonsterSpawnCenter").transform;
            spawnCenter.position = new Vector3(0f, 0f, 6f);
            EnemySpawnPanel panel = new GameObject("EnemyGymPanel").AddComponent<EnemySpawnPanel>();
            panel.Configure(EnemyDataBuilder.SeasonOne(), EnemyDataBuilder.EnsureMixedGroup(), EnemyDataBuilder.EnsureKankrelatGroup(), enemyProjectile, spawnCenter);
            EditorUtility.SetDirty(panel);

            NavMeshBakeUtility.PrepareEnemyScene("EnemyGym");

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Enemy gym generated at " + ScenePath);
        }

        private static void BuildArena(Material floor, Material accent, Material wall, Material sea)
        {
            // 44 x 44 floor (top at y = 0), walls on three sides, open south edge over the Digital Sea.
            CodeLyokoSceneBuilder.CreatePlatform("Gym_Floor", new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), floor);
            CodeLyokoSceneBuilder.CreatePlatform("Gym_Wall_N", new Vector3(0f, 1.5f, 22.5f), new Vector3(46f, 3f, 1f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Gym_Wall_E", new Vector3(22.5f, 1.5f, 0f), new Vector3(1f, 3f, 46f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Gym_Wall_W", new Vector3(-22.5f, 1.5f, 0f), new Vector3(1f, 3f, 46f), wall);

            GameObject seaPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seaPlane.name = "DigitalSea_South";
            seaPlane.transform.position = new Vector3(0f, -9f, -40f);
            seaPlane.transform.localScale = new Vector3(70f, 0.3f, 36f);
            seaPlane.GetComponent<Renderer>().sharedMaterial = sea;
            Object.DestroyImmediate(seaPlane.GetComponent<Collider>());

            // Plateau (top at y = 2) reached by a 14-degree ramp from the south: tests NavMesh paths.
            CodeLyokoSceneBuilder.CreatePlatform("Plateau", new Vector3(12f, 1f, 10f), new Vector3(8f, 2f, 8f), accent);
            GameObject ramp = CodeLyokoSceneBuilder.CreatePlatform("Plateau_Ramp", new Vector3(12f, 0.9f, 2.2f), new Vector3(4f, 0.4f, 8.2f), accent);
            ramp.transform.rotation = Quaternion.Euler(-14.5f, 0f, 0f);

            // Cover to break line of sight and flank the Blok.
            CodeLyokoSceneBuilder.CreatePlatform("Cover_A", new Vector3(-8f, 1f, 2f), new Vector3(4f, 2f, 1f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Cover_B", new Vector3(-12f, 1f, 12f), new Vector3(1f, 2f, 4f), wall);
            CodeLyokoSceneBuilder.CreatePlatform("Pillar", new Vector3(5f, 1.5f, -4f), new Vector3(1.2f, 3f, 1.2f), wall);
        }
    }
}
