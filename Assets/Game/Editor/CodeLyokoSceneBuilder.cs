using System.IO;
using Cinemachine;
using CodeLyokoFanGame;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    public static class CodeLyokoSceneBuilder
    {
        private const string GeneratedFolder = "Assets/Game/Generated";
        private const string PrefabFolder = GeneratedFolder + "/Prefabs";
        private const string MaterialFolder = GeneratedFolder + "/Materials";
        private const string ScenePath = ProjectScenes.ForestScenePath;

        [MenuItem("Tools/Code Lyoko Fan Game/Build Vertical Slice Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            UrpPipelineSetup.EnsurePipeline();
            EnsureTag("Player");
            EnsureTag("Enemy");
            PhysicsLayersSetup.EnsureLayers();
            InputReader input = CoreAssetsBuilder.EnsureInputReader();
            CoreAssetsBuilder.EnsureEventSet();
            MissionDefinition mission = PrototypeDataAssetBuilder.EnsureForestMission();

            Material forest = Material("Lyoko_Forest_Green", SectorArtBuilder.Sector(SectorId.Forest).groundColor, 0f);
            Material dark = Material("Lyoko_Dark_Metal", new Color(0.06f, 0.08f, 0.09f), 0.1f);
            Material cyan = Material("Lyoko_Cyan_Emission", new Color(0.1f, 0.85f, 1f), 1.7f);
            Material red = Material("Xana_Red_Emission", new Color(1f, 0.06f, 0.04f), 2.2f);
            Material violet = Material("Odd_Violet", new Color(0.45f, 0.18f, 0.95f), 0.4f);
            Material green = Material("Ulrich_Green", new Color(0.06f, 0.42f, 0.2f), 0.3f);
            Material black = Material("Yumi_Black", new Color(0.02f, 0.02f, 0.025f), 0.2f);
            Material pink = Material("Aelita_Pink", new Color(1f, 0.42f, 0.78f), 0.8f);
            Material orange = Material("Xana_Orange", new Color(1f, 0.38f, 0.03f), 1.4f);

            Projectile playerProjectile = BuildProjectilePrefab("PlayerEnergyDart", cyan, false);
            Projectile enemyProjectile = BuildProjectilePrefab("XanaLaser", red, true);
            VehicleController overboard = BuildVehiclePrefab("Overboard", VehicleStyle.Overboard, violet, PrimitiveType.Cube);
            VehicleController overbike = BuildVehiclePrefab("Overbike", VehicleStyle.Overbike, green, PrimitiveType.Cylinder);
            VehicleController overwing = BuildVehiclePrefab("Overwing", VehicleStyle.Overwing, black, PrimitiveType.Cube);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject systems = new GameObject("GameSystems");
            LockOnSystem lockOn = systems.AddComponent<LockOnSystem>();
            CharacterSwitcher switcher = systems.AddComponent<CharacterSwitcher>();
            MissionRunner missionRunner = systems.AddComponent<MissionRunner>();
            GameHud hud = systems.AddComponent<GameHud>();
            DigitalSeaHazard sea = systems.AddComponent<DigitalSeaHazard>();
            systems.AddComponent<MissionFailScreen>().Configure(missionRunner);

            CameraRig cameraRig = CreateCameraRig(lockOn, input);
            SetPrivate(switcher, "cameraRig", cameraRig);
            SetPrivate(switcher, "lockOnSystem", lockOn);
            SetPrivate(switcher, "input", input);
            SetPrivate(lockOn, "input", input);

            CreateLighting();
            CreateLevelGeometry(forest, dark, cyan);

            Transform respawn = new GameObject("RespawnPoint").transform;
            respawn.position = new Vector3(0f, 4f, -12f);
            sea.Configure(false, respawn);
            // One checkpoint per stretch of the mission; each step activates its own (MissionStep.checkpointId).
            CreateCheckpoint("StartCheckpoint", "start", new Vector3(0f, 1f, -14f));
            CreateCheckpoint("BridgeCheckpoint", "bridge", new Vector3(0f, 1f, 8f));
            CreateCheckpoint("PlateauCheckpoint", "plateau", new Vector3(0f, 1f, 34f));
            CreateCheckpoint("TowerCheckpoint", "tower", new Vector3(0f, 1f, 40f));

            CreateMissionMarker("BridgeMissionMarker", "bridge", new Vector3(0f, 1f, 8f));
            CreateMissionMarker("TowerMissionMarker", "tower", new Vector3(0f, 1f, 45f));

            PlayerController odd = CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));
            PlayerController ulrich = CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));
            PlayerController yumi = CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));

            switcher.Register(odd);
            switcher.Register(ulrich);
            switcher.Register(yumi);

            AelitaCompanion aelita = CreateAelita(pink, switcher, new Vector3(-2.2f, 2f, -13f));
            SetPrivate(aelita, "energyFieldPrefab", BuildProjectilePrefab("AelitaEnergyField", pink, false, true));
            systems.AddComponent<AelitaOrders>().Configure(aelita, switcher, input);
            // F07: tower prefab with its halo, the interior where Aelita types CODE: LYOKO, and a passage tower pair
            // between the start and a small test island.
            GameObject towerPrefab = TowerBuilder.BuildTowerPrefab();
            TowerInterior interior = TowerBuilder.CreateInterior(new Vector3(220f, 0.5f, 50f), switcher, input);
            TowerObjective tower = TowerBuilder.CreateActivatedTower(towerPrefab, new Vector3(0f, 0.5f, 50f), aelita, input, interior);
            CreatePlatform("PassageIsland", new Vector3(-150f, 0f, 0f), new Vector3(16f, 1f, 16f), forest);
            TowerBuilder.CreatePassagePair(towerPrefab, "PassageTower_Bosque", new Vector3(-9f, 0.5f, -20f), "PassageTower_Isla", new Vector3(-150f, 0.5f, -4f));
            missionRunner.Configure(mission, tower, switcher, input, aelita);
            EditorUtility.SetDirty(missionRunner);
            hud.Configure(switcher, missionRunner, tower, aelita);

            CreateEnemy("Kankrelat_A", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(3f, 0.5f, 4f));
            CreateEnemy("Kankrelat_B", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(-3f, 0.5f, 9f));
            CreateEnemy("Blok_A", EnemyKind.Blok, dark, red, enemyProjectile, new Vector3(10f, 0.5f, 22f));
            CreateEnemy("Krab_A", EnemyKind.Krab, dark, red, enemyProjectile, new Vector3(-10f, 0.5f, 24f));
            CreateEnemy("Hornet_A", EnemyKind.Hornet, orange, red, enemyProjectile, new Vector3(8f, 5f, 27f));
            CreateEnemy("Megatank_Boss", EnemyKind.Megatank, dark, red, enemyProjectile, new Vector3(0f, 0.9f, 43f));
            CreateWaveSpawner("TowerReinforcementWave", enemyProjectile, new Vector3(0f, 0.5f, 35f));
            NavMeshBakeUtility.PrepareEnemyScene("ForestSectorVerticalSlice");

            EditorSceneManager.SaveScene(scene, ScenePath);
            ProjectScenes.RefreshBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);

            Debug.Log("Code Lyoko vertical slice generated at " + ScenePath);
        }

        internal static void EnsureFolders()
        {
            Directory.CreateDirectory(GeneratedFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory("Assets/Levels");
        }

        internal static void EnsureTag(string tag)
        {
            string[] tags = InternalEditorUtility.tags;
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag)
                {
                    return;
                }
            }

            InternalEditorUtility.AddTag(tag);
        }

        internal static Material Material(string name, Color color, float emission)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            // F09: every generated material uses the Lyoko toon shader.
            Shader toon = Shader.Find(LyokoMaterials.ToonShader) ?? Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(toon);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = toon;
            LyokoMaterials.ConfigureToon(material, color, emission, 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        // round = glowing sphere (Aelita's energy field); otherwise an elongated bolt (lasers, arrows).
        internal static Projectile BuildProjectilePrefab(string name, Material material, bool hostile, bool round = false)
        {
            string path = PrefabFolder + "/" + name + ".prefab";
            string layer = hostile ? GameLayers.EnemyAttack : GameLayers.PlayerAttack;
            Projectile existing = AssetDatabase.LoadAssetAtPath<Projectile>(path);
            if (existing != null)
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                StyleProjectile(contents, material, hostile, round);
                GameLayers.SetRecursively(contents, layer);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);
                return AssetDatabase.LoadAssetAtPath<Projectile>(path);
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = name;
            Collider collider = root.GetComponent<Collider>();
            collider.isTrigger = true;
            root.AddComponent<Rigidbody>().isKinematic = true;
            root.AddComponent<Projectile>();
            StyleProjectile(root, material, hostile, round);
            GameLayers.SetRecursively(root, layer);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<Projectile>();
        }

        // F09 (F9.23): glowing faceted shot with a light trail in its own colour.
        private static void StyleProjectile(GameObject root, Material material, bool hostile, bool round)
        {
            Color color = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            root.transform.localScale = Vector3.one * (round ? 0.42f : hostile ? 0.28f : 0.22f);
            root.GetComponent<MeshFilter>().sharedMesh = round
                ? LowPolyMesh.Gem(0.5f, 1)
                : LowPolyMesh.Get("ProjectileBolt", b => b.Gem(Matrix4x4.Scale(new Vector3(0.7f, 0.7f, 1.9f)), 0.5f, 1));
            MeshRenderer renderer = root.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = LyokoMaterials.Toon(color, 2.5f);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (!root.TryGetComponent(out TrailRenderer trail))
            {
                trail = root.AddComponent<TrailRenderer>();
            }

            trail.time = round ? 0.25f : 0.14f;
            trail.startWidth = round ? 0.35f : hostile ? 0.26f : 0.2f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.05f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Color glow = color;
            glow.a = 0.8f;
            trail.sharedMaterial = LyokoMaterials.Glow(glow, 0f, 0.5f);
        }

        // Rebuilt on every run so handling and visuals follow VehiclePrefabBuilder (F08).
        internal static VehicleController BuildVehiclePrefab(string name, VehicleStyle style, Material material, PrimitiveType primitive)
        {
            return VehiclePrefabBuilder.Build(name, style, material);
        }

        internal static CameraRig CreateCameraRig(LockOnSystem lockOn, InputReader input)
        {
            GameObject cameraObject = new GameObject("MainCamera");
            cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 7f, -20f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.GetComponent<Camera>().GetUniversalAdditionalCameraData().renderPostProcessing = true;
            CinemachineBrain brain = cameraObject.AddComponent<CinemachineBrain>();
            brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.35f);

            GameObject rigObject = new GameObject("CameraRig");
            CameraRig rig = rigObject.AddComponent<CameraRig>();
            CinemachineImpulseSource impulse = rigObject.AddComponent<CinemachineImpulseSource>();
            impulse.m_ImpulseDefinition.m_ImpulseChannel = 1;
            impulse.m_ImpulseDefinition.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
            impulse.m_ImpulseDefinition.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            impulse.m_ImpulseDefinition.m_ImpulseDuration = 0.2f;
            impulse.m_DefaultVelocity = new Vector3(0.15f, -0.5f, 0f);

            CinemachineFreeLook explore = CreateFreeLook("CM_Explore", rigObject.transform, 50f,
                new CinemachineFreeLook.Orbit(4.2f, 2.6f), new CinemachineFreeLook.Orbit(2f, 6f), new CinemachineFreeLook.Orbit(0.4f, 3.4f));
            CinemachineFreeLook vehicle = CreateFreeLook("CM_Vehicle", rigObject.transform, 56f,
                new CinemachineFreeLook.Orbit(6f, 4f), new CinemachineFreeLook.Orbit(3.2f, 9f), new CinemachineFreeLook.Orbit(1f, 5f));
            vehicle.Priority = 5;

            Transform pivot = new GameObject("LockOnPivot").transform;
            pivot.SetParent(rigObject.transform);
            CinemachineTargetGroup group = new GameObject("LockOnGroup").AddComponent<CinemachineTargetGroup>();
            group.transform.SetParent(rigObject.transform);

            GameObject lockOnObject = new GameObject("CM_LockOn");
            lockOnObject.transform.SetParent(rigObject.transform);
            CinemachineVirtualCamera lockOnCamera = lockOnObject.AddComponent<CinemachineVirtualCamera>();
            lockOnCamera.Priority = 5;
            lockOnCamera.m_Lens.FieldOfView = 52f;
            lockOnCamera.Follow = pivot;
            lockOnCamera.LookAt = group.transform;
            CinemachineTransposer body = lockOnCamera.AddCinemachineComponent<CinemachineTransposer>();
            body.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetNoRoll;
            body.m_FollowOffset = new Vector3(1.2f, 3f, -7f);
            CinemachineGroupComposer aim = lockOnCamera.AddCinemachineComponent<CinemachineGroupComposer>();
            aim.m_GroupFramingSize = 0.6f;
            AddCameraExtensions(lockOnObject);

            SetPrivate(rig, "input", input);
            SetPrivate(rig, "lockOn", lockOn);
            SetPrivate(rig, "exploreCamera", explore);
            SetPrivate(rig, "vehicleCamera", vehicle);
            SetPrivate(rig, "lockOnCamera", lockOnCamera);
            SetPrivate(rig, "lockOnGroup", group);
            SetPrivate(rig, "lockOnPivot", pivot);
            SetPrivate(rig, "impulseSource", impulse);
            return rig;
        }

        private static CinemachineFreeLook CreateFreeLook(string name, Transform parent, float fov, CinemachineFreeLook.Orbit top, CinemachineFreeLook.Orbit middle, CinemachineFreeLook.Orbit bottom)
        {
            GameObject cameraObject = new GameObject(name);
            cameraObject.transform.SetParent(parent);
            CinemachineFreeLook freeLook = cameraObject.AddComponent<CinemachineFreeLook>();
            freeLook.Priority = 10;
            freeLook.m_Lens.FieldOfView = fov;
            freeLook.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
            freeLook.m_Orbits = new[] { top, middle, bottom };
            freeLook.m_Transitions.m_InheritPosition = true;

            // CameraRig feeds look input every frame (InputReader); gains are degrees / orbit fraction per unit.
            ConfigureAxis(ref freeLook.m_XAxis, 3.2f);
            ConfigureAxis(ref freeLook.m_YAxis, 0.035f);
            freeLook.m_YAxis.Value = 0.5f;

            AddCameraExtensions(cameraObject);
            return freeLook;
        }

        private static void ConfigureAxis(ref AxisState axis, float gain)
        {
            axis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
            axis.m_MaxSpeed = gain;
            axis.m_AccelTime = 0f;
            axis.m_DecelTime = 0f;
            axis.m_InputAxisName = string.Empty;
            axis.m_InvertInput = false;
        }

        private static void AddCameraExtensions(GameObject cameraObject)
        {
            CinemachineCollider collider = cameraObject.AddComponent<CinemachineCollider>();
            collider.m_CollideAgainst = LayerMask.GetMask("Default", GameLayers.Environment);
            collider.m_IgnoreTag = "Player";
            collider.m_MinimumDistanceFromTarget = 0.6f;
            collider.m_AvoidObstacles = true;
            collider.m_CameraRadius = 0.25f;

            CinemachineImpulseListener listener = cameraObject.AddComponent<CinemachineImpulseListener>();
            listener.m_ChannelMask = 1;
            listener.m_Gain = 1f;
        }

        internal static void CreateLighting()
        {
            CreateLighting(SectorId.Forest);
        }

        // Sun with soft shadows (the toon shader turns them into flat bands) plus the sector look (F09).
        internal static void CreateLighting(SectorId sector)
        {
            GameObject sun = new GameObject("VirtualSun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.98f, 0.94f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.85f;
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            LookBuilder.ApplyToScene(sector);
        }

        private static void CreateLevelGeometry(Material forest, Material dark, Material cyan)
        {
            GameObject[] platforms =
            {
                CreatePlatform("StartPlateau", new Vector3(0f, 0f, -12f), new Vector3(24f, 1f, 20f), forest),
                CreatePlatform("BridgeDataPath", new Vector3(0f, 0f, 8f), new Vector3(9f, 1f, 24f), forest),
                CreatePlatform("CombatPlateau", new Vector3(0f, 0f, 28f), new Vector3(30f, 1f, 24f), forest),
                CreatePlatform("TowerPlateau", new Vector3(0f, 0f, 48f), new Vector3(22f, 1f, 18f), forest)
            };

            // F09: cliffs under the paths, giant trees rising from the Digital Sea and floating data.
            SectorDefinition look = SectorArtBuilder.Sector(SectorId.Forest);
            for (int i = 0; i < platforms.Length; i++)
            {
                SectorArtBuilder.AddCliff(platforms[i], look.terrainColor, 7f);
            }

            SectorArtBuilder.DressForest(look);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 11f, 1.2f, 48f + Mathf.Sin(angle) * 8f);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "DataTree_" + i;
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(0.45f, 2.4f + (i % 3), 0.45f);
                pillar.GetComponent<Renderer>().sharedMaterial = i % 2 == 0 ? dark : cyan;
                // F09: faceted crystal column with a glowing tip.
                pillar.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Get("DataTreeColumn", b =>
                {
                    b.Prism(Matrix4x4.Translate(new Vector3(0f, -1f, 0f)), 6, 0.5f, 0.35f, 1.8f);
                    b.Prism(Matrix4x4.Translate(new Vector3(0f, 0.8f, 0f)), 6, 0.35f, 0f, 0.4f);
                });
                GameLayers.SetRecursively(pillar, GameLayers.Environment);
            }

            SectorArtBuilder.CreateDigitalSea(new Vector3(0f, -8.2f, 20f), 360f);
        }

        internal static GameObject CreatePlatform(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.position = position;
            platform.transform.localScale = scale;
            platform.GetComponent<Renderer>().sharedMaterial = material;
            // F09: chamfered slab instead of the plain cube; the BoxCollider keeps the exact size.
            platform.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Slab(scale);
            GameLayers.SetRecursively(platform, GameLayers.Environment);
            return platform;
        }

        private static void CreateCheckpoint(string name, string id, Vector3 position)
        {
            GameObject checkpoint = new GameObject(name);
            checkpoint.transform.position = position;
            checkpoint.AddComponent<Checkpoint>().SetId(id);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "CheckpointMarker";
            marker.transform.SetParent(checkpoint.transform);
            marker.transform.localPosition = Vector3.up * 0.04f;
            marker.transform.localScale = new Vector3(1.8f, 1f, 1.8f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            // F09: a ring of light on the ground instead of a grey disc.
            marker.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.02f, 24, 4);
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.sharedMaterial = LyokoMaterials.Glow(new Color(0.35f, 0.9f, 1f));
            markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void CreateMissionMarker(string name, string id, Vector3 position)
        {
            GameObject markerObject = new GameObject(name);
            markerObject.transform.position = position;
            markerObject.AddComponent<MissionMarker>().Configure(id, 6f);
        }

        private static void CreateWaveSpawner(string name, Projectile enemyProjectile, Vector3 position)
        {
            GameObject spawnerObject = new GameObject(name);
            spawnerObject.transform.position = position;
            WaveSpawner spawner = spawnerObject.AddComponent<WaveSpawner>();
            SetPrivate(spawner, "enemyProjectilePrefab", enemyProjectile);
            spawner.SetWaves(EnemyDataBuilder.EnsureForestWaves());
        }

        internal static PlayerController CreatePlayer(string name, LyokoCharacter id, AttackStyle style, VehicleStyle vehicleStyle, Material material, VehicleController vehicle, Projectile projectile, CharacterSwitcher switcher, LockOnSystem lockOn, Vector3 position)
        {
            GameObject root = new GameObject(name);
            root.tag = "Player";
            root.transform.position = position;
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.center = Vector3.up;
            Health health = root.AddComponent<Health>();
            health.SetMaxHealth(100f);
            health.SetDestroyOnDeath(false);
            PlayerCombat combat = root.AddComponent<PlayerCombat>();
            PlayerController player = root.AddComponent<PlayerController>();

            SetPrivate(player, "character", id);
            SetPrivate(player, "vehicleStyle", vehicleStyle);
            SetPrivate(player, "vehiclePrefab", vehicle);
            GameObject vehicleSocket = new GameObject("VehicleSocket");
            vehicleSocket.transform.SetParent(root.transform, false);
            SetPrivate(player, "vehicleSocket", vehicleSocket.transform);
            SetPrivate(player, "definition", AssetDatabase.LoadAssetAtPath<CharacterDefinition>("Assets/Game/Data/Characters/" + id + ".asset"));
            SetPrivate(player, "movement", PrototypeDataAssetBuilder.EnsureMovementProfile());

            GameObject cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(root.transform);
            cameraTarget.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            combat.Configure(style, projectile, lockOn);
            CombatProfile combatProfile = PrototypeDataAssetBuilder.EnsureCombatProfile();
            SetPrivate(combat, "profile", combatProfile);
            root.AddComponent<SpecialMeter>();
            root.AddComponent<PlayerGuard>().Configure(combatProfile);
            root.AddComponent<AbilityController>().SetSquadProfile(AbilityDataBuilder.EnsureSquadProfile());
            root.AddComponent<CompanionAI>().enabled = false;

            // F09: low-poly avatar on the common rig, animated from code.
            root.AddComponent<ProceduralAnimator>().SetRig(WarriorModel.Build(id, root.transform));

            root.AddComponent<DamageFlicker>();
            GameLayers.SetRecursively(root, GameLayers.Player);
            return player;
        }

        internal static AelitaCompanion CreateAelita(Material material, CharacterSwitcher switcher, Vector3 position)
        {
            GameObject root = new GameObject("Aelita_Companion");
            root.transform.position = position;
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.34f;
            controller.center = Vector3.up * 0.9f;
            Health aelitaHealth = root.AddComponent<Health>();
            aelitaHealth.SetMaxHealth(100f);
            aelitaHealth.SetDestroyOnDeath(false);
            aelitaHealth.SetHitInvulnerability(0.5f);
            AelitaCompanion companion = root.AddComponent<AelitaCompanion>();
            GameLayers.SetRecursively(root, GameLayers.Player);
            SetPrivate(companion, "switcher", switcher);

            root.AddComponent<ProceduralAnimator>().SetRig(WarriorModel.Build(LyokoCharacter.Aelita, root.transform));
            GameLayers.SetRecursively(root, GameLayers.Player);
            return companion;
        }

        // F06: shared placeholder rig (EnemyRigBuilder) configured from the EnemyDefinition asset.
        internal static EnemyAI CreateEnemy(string name, EnemyKind kind, Material bodyMaterial, Material eyeMaterial, Projectile projectile, Vector3 position)
        {
            EnemyAI ai = EnemyRigBuilder.Build(name, kind, position, bodyMaterial, eyeMaterial);
            EnemyDefinition definition = EnemyDataBuilder.Find(kind);
            if (definition != null)
            {
                ai.Configure(definition, projectile);
            }
            else
            {
                ai.Configure(kind, projectile);
            }

            EditorUtility.SetDirty(ai);
            return ai;
        }

        internal static void SetPrivate<T>(Object target, string fieldName, T value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Field not found: " + target.GetType().Name + "." + fieldName);
                return;
            }

            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }
    }
}
