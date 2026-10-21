using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    // Test scene for movement and camera feel (F03): ramps, stairs, gaps over the Digital Sea,
    // climbable walls, moving platforms and lock-on dummies. Not part of the build.
    public static class MovementGymBuilder
    {
        public const string ScenePath = "Assets/Scenes/Test/MovementGym.unity";

        [MenuItem("Tools/Code Lyoko Fan Game/Build Movement Gym Scene")]
        public static void BuildMovementGym()
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
            respawn.position = new Vector3(0f, 2f, -25f);
            sea.Configure(true, respawn);

            BuildFloorAndPit(floor, cyan);
            BuildRamps(accent);
            BuildStairs(accent);
            BuildGapCourse(accent);
            BuildClimbWalls(wall);
            BuildMovingPlatforms(cyan, wall);

            Vector3 spawn = new Vector3(0f, 1f, -25f);
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, spawn));
            switcher.Register(CodeLyokoSceneBuilder.CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, spawn));
            hud.Configure(switcher, null, null, null);

            // Harmless lock-on dummies: turrets with no projectile never shoot.
            CodeLyokoSceneBuilder.CreateEnemy("LockOnDummy_A", EnemyKind.Blok, wall, red, null, new Vector3(-6f, 0.5f, 6f));
            CodeLyokoSceneBuilder.CreateEnemy("LockOnDummy_B", EnemyKind.Blok, wall, red, null, new Vector3(6f, 0.5f, 6f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Movement gym generated at " + ScenePath);
        }

        private static void BuildFloorAndPit(Material floor, Material sea)
        {
            // Main floor (top at y = 0) with a pit on the east side, x 10..30.
            CodeLyokoSceneBuilder.CreatePlatform("Floor_Main", new Vector3(-12.5f, -0.5f, 0f), new Vector3(45f, 1f, 70f), floor);
            CodeLyokoSceneBuilder.CreatePlatform("Floor_East", new Vector3(32.5f, -0.5f, 0f), new Vector3(5f, 1f, 70f), floor);

            GameObject seaPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seaPlane.name = "DigitalSea_Pit";
            seaPlane.transform.position = new Vector3(20f, -9f, 0f);
            seaPlane.transform.localScale = new Vector3(20f, 0.3f, 70f);
            seaPlane.GetComponent<Renderer>().sharedMaterial = sea;
            Object.DestroyImmediate(seaPlane.GetComponent<Collider>());
        }

        private static void BuildRamps(Material material)
        {
            // 15 and 30 degrees are walkable; 50 is above the CharacterController slope limit (45).
            CreateRamp("Ramp_15deg", -26f, 15f, material);
            CreateRamp("Ramp_30deg", -20f, 30f, material);
            CreateRamp("Ramp_50deg", -14f, 50f, material);
        }

        private static void CreateRamp(string name, float x, float angle, Material material)
        {
            const float length = 10f;
            float radians = angle * Mathf.Deg2Rad;
            float rise = Mathf.Sin(radians) * length;
            float run = Mathf.Cos(radians) * length;
            GameObject ramp = CodeLyokoSceneBuilder.CreatePlatform(name, new Vector3(x, rise * 0.5f, -12f + run * 0.5f), new Vector3(4f, 0.4f, length), material);
            ramp.transform.rotation = Quaternion.Euler(-angle, 0f, 0f);
        }

        private static void BuildStairs(Material material)
        {
            // 0.2 m steps climb on their own; 0.4 m is above the step offset (0.3) and needs jumps.
            CreateStairs("Stairs_Low", -6f, 0.2f, 8, material);
            CreateStairs("Stairs_High", 0f, 0.4f, 5, material);
        }

        private static void CreateStairs(string name, float x, float stepHeight, int steps, Material material)
        {
            const float depth = 0.7f;
            for (int i = 0; i < steps; i++)
            {
                float height = stepHeight * (i + 1);
                CodeLyokoSceneBuilder.CreatePlatform(name + "_" + i, new Vector3(x, height * 0.5f, -14f + i * depth), new Vector3(4f, height, depth), material);
            }
        }

        private static void BuildGapCourse(Material material)
        {
            // Over the pit: gaps of 2 m (normal jump), 4 m (sprint jump), 6 m (double jump / long jump).
            CodeLyokoSceneBuilder.CreatePlatform("Gap_Start", new Vector3(20f, -0.5f, -27.5f), new Vector3(20f, 1f, 15f), material);
            CodeLyokoSceneBuilder.CreatePlatform("Gap_2m", new Vector3(20f, -0.5f, -15f), new Vector3(6f, 1f, 6f), material);
            CodeLyokoSceneBuilder.CreatePlatform("Gap_4m", new Vector3(20f, -0.5f, -5f), new Vector3(6f, 1f, 6f), material);
            CodeLyokoSceneBuilder.CreatePlatform("Gap_6m_End", new Vector3(20f, -0.5f, 19.5f), new Vector3(20f, 1f, 31f), material);
        }

        private static void BuildClimbWalls(Material material)
        {
            // Odd climbs: jump at a wall and hold forward.
            float[] heights = { 3f, 6f, 10f };
            for (int i = 0; i < heights.Length; i++)
            {
                float h = heights[i];
                float x = -28f + i * 6f;
                CodeLyokoSceneBuilder.CreatePlatform("ClimbWall_" + h + "m", new Vector3(x, h * 0.5f, 20f), new Vector3(4f, h, 1f), material);
                CodeLyokoSceneBuilder.CreatePlatform("ClimbWall_" + h + "m_Top", new Vector3(x, h - 0.25f, 21.5f), new Vector3(4f, 0.5f, 2.5f), material);
            }
        }

        private static void BuildMovingPlatforms(Material moving, Material ledge)
        {
            GameObject sideways = CodeLyokoSceneBuilder.CreatePlatform("MovingPlatform_Sideways", new Vector3(-4f, 0.25f, 22f), new Vector3(4f, 0.5f, 4f), moving);
            sideways.AddComponent<MovingPlatform>().Configure(new Vector3(8f, 0f, 0f), 6f);

            GameObject elevator = CodeLyokoSceneBuilder.CreatePlatform("MovingPlatform_Elevator", new Vector3(8f, 0.25f, 20f), new Vector3(3f, 0.5f, 3f), moving);
            elevator.AddComponent<MovingPlatform>().Configure(new Vector3(0f, 6f, 0f), 7f);

            // High ledge next to the elevator top, to test edge protection.
            CodeLyokoSceneBuilder.CreatePlatform("HighLedge", new Vector3(8f, 5.75f, 25f), new Vector3(5f, 0.5f, 7f), ledge);
        }
    }
}
