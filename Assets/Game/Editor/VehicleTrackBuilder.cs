using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    // Test scene for vehicles (F08), later the base of the race mode: a ground loop around a Digital Sea lake
    // with a jump over the sea, an elevated section, ramming / shooting dummies, floating gates for the flyers
    // and Aelita at the start to try the Overwing passenger seat. Not part of the build.
    public static class VehicleTrackBuilder
    {
        public const string ScenePath = "Assets/Scenes/Test/VehicleTrack.unity";

        private const float TrackWidth = 16f;
        private const int IgnoreRaycastLayer = 2;

        [MenuItem("Tools/Code Lyoko Fan Game/Build Vehicle Track Scene")]
        public static void BuildVehicleTrack()
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
            Material orange = CodeLyokoSceneBuilder.Material("Xana_Orange", new Color(1f, 0.38f, 0.03f), 1.4f);
            Material violet = CodeLyokoSceneBuilder.Material("Odd_Violet", new Color(0.45f, 0.18f, 0.95f), 0.4f);
            Material green = CodeLyokoSceneBuilder.Material("Ulrich_Green", new Color(0.06f, 0.42f, 0.2f), 0.3f);
            Material black = CodeLyokoSceneBuilder.Material("Yumi_Black", new Color(0.02f, 0.02f, 0.025f), 0.2f);
            Material pink = CodeLyokoSceneBuilder.Material("Aelita_Pink", new Color(1f, 0.42f, 0.78f), 0.8f);

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

            Vector3 spawn = new Vector3(-20f, 1f, 0f);
            Transform respawn = new GameObject("RespawnPoint").transform;
            respawn.position = spawn + Vector3.up;
            sea.Configure(true, respawn);

            BuildLoop(floor, accent, cyan);
            BuildSea(cyan);
            BuildObstacles(wall);
            BuildGates(cyan);

            CreateDummy("RamDummy_A", new Vector3(25f, 0f, 125f), orange, null);
            CreateDummy("RamDummy_B", new Vector3(5f, 0f, 131f), orange, null);
            CreateDummy("RamDummy_C", new Vector3(-15f, 0f, 126f), orange, null);
            CreateDummy("TurretDummy_North", new Vector3(35f, 0f, 133f), wall, enemyProjectile);
            CreateDummy("TurretDummy_West", new Vector3(-62f, 0f, 112f), wall, enemyProjectile);

            Quaternion facing = Quaternion.Euler(0f, 90f, 0f);
            PlayerController odd = CodeLyokoSceneBuilder.CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, spawn);
            PlayerController ulrich = CodeLyokoSceneBuilder.CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, spawn);
            PlayerController yumi = CodeLyokoSceneBuilder.CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, spawn);
            odd.transform.rotation = facing;
            ulrich.transform.rotation = facing;
            yumi.transform.rotation = facing;
            switcher.Register(odd);
            switcher.Register(ulrich);
            switcher.Register(yumi);

            AelitaCompanion aelita = CodeLyokoSceneBuilder.CreateAelita(pink, switcher, spawn + new Vector3(-3f, 0f, 3f));
            hud.Configure(switcher, null, null, aelita);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Vehicle track generated at " + ScenePath);
        }

        // Counter-clockwise loop, top of the track at y = 0:
        // south straight (start) -> east straight with a jump over the sea -> north straight (dummies)
        // -> west straight with an elevated section -> back to the start.
        private static void BuildLoop(Material floor, Material corner, Material glow)
        {
            CodeLyokoSceneBuilder.CreatePlatform("Track_South", new Vector3(0f, -0.5f, 0f), new Vector3(132f, 1f, TrackWidth), floor);
            CodeLyokoSceneBuilder.CreatePlatform("Track_North", new Vector3(0f, -0.5f, 128f), new Vector3(132f, 1f, TrackWidth), floor);

            CodeLyokoSceneBuilder.CreatePlatform("Track_East_A", new Vector3(58f, -0.5f, 24f), new Vector3(TrackWidth, 1f, 32f), floor);
            CreateRamp("Track_East_JumpRamp", new Vector3(58f, 0f, 40f), new Vector3(58f, 2.5f, 52f), 12f, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Track_East_B", new Vector3(58f, -0.5f, 90f), new Vector3(TrackWidth, 1f, 60f), floor);

            CodeLyokoSceneBuilder.CreatePlatform("Track_West_A", new Vector3(-58f, -0.5f, 110f), new Vector3(TrackWidth, 1f, 20f), floor);
            CreateRamp("Track_West_RampUp", new Vector3(-58f, 0f, 100f), new Vector3(-58f, 4f, 88f), 12f, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Track_West_High", new Vector3(-58f, 3.5f, 64f), new Vector3(12f, 1f, 48f), floor);
            CreateRamp("Track_West_RampDown", new Vector3(-58f, 4f, 40f), new Vector3(-58f, 0f, 28f), 12f, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Track_West_B", new Vector3(-58f, -0.5f, 18f), new Vector3(TrackWidth, 1f, 20f), floor);

            // Wide corners (slightly lower to avoid flickering with the straights) leave room to drift.
            Vector3 cornerSize = new Vector3(28f, 1f, 28f);
            CodeLyokoSceneBuilder.CreatePlatform("Corner_SE", new Vector3(56f, -0.51f, 4f), cornerSize, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Corner_NE", new Vector3(56f, -0.51f, 124f), cornerSize, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Corner_NW", new Vector3(-56f, -0.51f, 124f), cornerSize, corner);
            CodeLyokoSceneBuilder.CreatePlatform("Corner_SW", new Vector3(-56f, -0.51f, 4f), cornerSize, corner);

            GameObject startLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            startLine.name = "StartLine";
            startLine.transform.position = new Vector3(0f, 0.01f, 0f);
            startLine.transform.localScale = new Vector3(0.6f, 0.02f, TrackWidth);
            startLine.GetComponent<Renderer>().sharedMaterial = glow;
            Object.DestroyImmediate(startLine.GetComponent<Collider>());
        }

        private static void BuildSea(Material material)
        {
            GameObject seaPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seaPlane.name = "DigitalSea";
            seaPlane.transform.position = new Vector3(0f, -10f, 64f);
            seaPlane.transform.localScale = new Vector3(400f, 0.3f, 400f);
            seaPlane.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(seaPlane.GetComponent<Collider>());
        }

        private static void BuildObstacles(Material material)
        {
            // Slalom on the north straight and pillars holding the elevated section.
            float[] slalomX = { -30f, -40f, -50f };
            for (int i = 0; i < slalomX.Length; i++)
            {
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "SlalomPillar_" + i;
                pillar.transform.position = new Vector3(slalomX[i], 2f, i % 2 == 0 ? 124f : 132f);
                pillar.transform.localScale = new Vector3(1.2f, 2f, 1.2f);
                pillar.GetComponent<Renderer>().sharedMaterial = material;
                GameLayers.SetRecursively(pillar, GameLayers.Environment);
            }

            for (int i = 0; i < 2; i++)
            {
                CodeLyokoSceneBuilder.CreatePlatform("HighSupport_" + i, new Vector3(-58f, -4f, 52f + i * 24f), new Vector3(2f, 14f, 2f), material);
            }
        }

        // Numbered in race order. Ground gates have no bottom bar so the Overbike can drive through.
        private static void BuildGates(Material material)
        {
            CreateGate(0, new Vector3(10f, 0f, 0f), 90f, false, material);
            CreateGate(1, new Vector3(40f, 7f, 40f), 20f, true, material);
            CreateGate(2, new Vector3(58f, 0f, 92f), 0f, false, material);
            CreateGate(3, new Vector3(25f, 12f, 80f), -40f, true, material);
            CreateGate(4, new Vector3(45f, 0f, 128f), -90f, false, material);
            CreateGate(5, new Vector3(0f, 16f, 104f), -90f, true, material);
            CreateGate(6, new Vector3(-58f, 4f, 64f), 180f, false, material);
            CreateGate(7, new Vector3(-30f, 9f, 60f), 160f, true, material);
            CreateGate(8, new Vector3(-36f, 5f, 24f), 120f, true, material);
        }

        private static void CreateGate(int index, Vector3 bottomCenter, float yaw, bool withBottomBar, Material material)
        {
            const float width = 8f;
            const float height = 6f;
            const float bar = 0.5f;

            GameObject root = new GameObject("Gate_" + index);
            root.layer = IgnoreRaycastLayer;
            root.transform.SetPositionAndRotation(bottomCenter, Quaternion.Euler(0f, yaw, 0f));

            Renderer[] frame = new Renderer[withBottomBar ? 4 : 3];
            frame[0] = GatePart(root.transform, "Post_L", new Vector3(-(width + bar) * 0.5f, height * 0.5f, 0f), new Vector3(bar, height + bar * 2f, bar), material);
            frame[1] = GatePart(root.transform, "Post_R", new Vector3((width + bar) * 0.5f, height * 0.5f, 0f), new Vector3(bar, height + bar * 2f, bar), material);
            frame[2] = GatePart(root.transform, "Top", new Vector3(0f, height + bar * 0.5f, 0f), new Vector3(width, bar, bar), material);
            if (withBottomBar)
            {
                frame[3] = GatePart(root.transform, "Bottom", new Vector3(0f, -bar * 0.5f, 0f), new Vector3(width, bar, bar), material);
            }

            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, height * 0.5f, 0f);
            trigger.size = new Vector3(width, height, 1.5f);
            root.AddComponent<TrackGate>().Configure(index, frame);
        }

        private static Renderer GatePart(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            GameLayers.SetRecursively(part, GameLayers.Environment);
            return part.GetComponent<Renderer>();
        }

        // Slab whose top surface goes from startTop to endTop.
        private static void CreateRamp(string name, Vector3 startTop, Vector3 endTop, float width, Material material)
        {
            const float thickness = 1f;
            Vector3 along = endTop - startTop;
            Quaternion rotation = Quaternion.LookRotation(along.normalized, Vector3.up);
            Vector3 center = (startTop + endTop) * 0.5f - rotation * Vector3.up * (thickness * 0.5f);
            GameObject ramp = CodeLyokoSceneBuilder.CreatePlatform(name, center, new Vector3(width, thickness, along.magnitude), material);
            ramp.transform.rotation = rotation;
        }

        // Cube on the Enemy team that respawns; with a projectile it shoots slow lasers at warriors.
        private static void CreateDummy(string name, Vector3 position, Material material, Projectile projectile)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.tag = "Enemy";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            body.transform.localScale = new Vector3(1.6f, 2.5f, 1.6f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Health health = body.AddComponent<Health>();
            health.SetTeam(Team.Enemy);
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(projectile != null ? 120f : 80f);

            GameLayers.SetRecursively(root, GameLayers.Enemy);
            root.AddComponent<TargetDummy>().Configure(health, projectile);
        }
    }
}
