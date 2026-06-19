using System.IO;
using CodeLyokoFanGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeLyokoFanGame.EditorTools
{
    public static class CodeLyokoSceneBuilder
    {
        private const string GeneratedFolder = "Assets/Game/Generated";
        private const string PrefabFolder = GeneratedFolder + "/Prefabs";
        private const string MaterialFolder = GeneratedFolder + "/Materials";
        private const string ScenePath = "Assets/Levels/ForestSectorVerticalSlice.unity";

        [MenuItem("Tools/Code Lyoko Fan Game/Build Vertical Slice Scene")]
        public static void BuildScene()
        {
            EnsureFolders();
            EnsureTag("Player");
            EnsureTag("Enemy");

            Material forest = Material("Lyoko_Forest_Green", new Color(0.08f, 0.7f, 0.33f), 0.2f);
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
            VehicleRig overboard = BuildVehiclePrefab("Overboard", VehicleStyle.Overboard, violet, PrimitiveType.Cube);
            VehicleRig overbike = BuildVehiclePrefab("Overbike", VehicleStyle.Overbike, green, PrimitiveType.Cylinder);
            VehicleRig overwing = BuildVehiclePrefab("Overwing", VehicleStyle.Overwing, black, PrimitiveType.Cube);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.skybox = null;
            RenderSettings.ambientLight = new Color(0.38f, 0.5f, 0.55f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.02f, 0.12f, 0.16f);
            RenderSettings.fogDensity = 0.012f;

            GameObject systems = new GameObject("GameSystems");
            LockOnSystem lockOn = systems.AddComponent<LockOnSystem>();
            CharacterSwitcher switcher = systems.AddComponent<CharacterSwitcher>();
            MissionManager mission = systems.AddComponent<MissionManager>();
            DigitalSeaHazard sea = systems.AddComponent<DigitalSeaHazard>();

            Camera camera = CreateCamera(lockOn);
            ThirdPersonCamera thirdPerson = camera.GetComponent<ThirdPersonCamera>();
            SetPrivate(switcher, "thirdPersonCamera", thirdPerson);
            SetPrivate(switcher, "lockOnSystem", lockOn);

            CreateLighting();
            CreateLevelGeometry(forest, dark, cyan);

            Transform respawn = new GameObject("RespawnPoint").transform;
            respawn.position = new Vector3(0f, 4f, -12f);
            SetPrivate(sea, "respawnPoint", respawn);

            PlayerController odd = CreatePlayer("Odd", LyokoCharacter.Odd, AttackStyle.Ranged, VehicleStyle.Overboard, violet, overboard, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));
            PlayerController ulrich = CreatePlayer("Ulrich", LyokoCharacter.Ulrich, AttackStyle.Melee, VehicleStyle.Overbike, green, overbike, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));
            PlayerController yumi = CreatePlayer("Yumi", LyokoCharacter.Yumi, AttackStyle.Telekinetic, VehicleStyle.Overwing, black, overwing, playerProjectile, switcher, lockOn, new Vector3(0f, 2f, -12f));

            switcher.Register(odd);
            switcher.Register(ulrich);
            switcher.Register(yumi);

            AelitaCompanion aelita = CreateAelita(pink, switcher, new Vector3(-2.2f, 2f, -13f));
            TowerObjective tower = CreateTower(cyan, red, aelita);
            SetPrivate(mission, "tower", tower);
            SetPrivate(mission, "switcher", switcher);

            CreateEnemy("Kankrelat_A", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(7f, 2f, 3f));
            CreateEnemy("Kankrelat_B", EnemyKind.Kankrelat, orange, red, enemyProjectile, new Vector3(-8f, 2f, 7f));
            CreateEnemy("Blok_A", EnemyKind.Blok, dark, red, enemyProjectile, new Vector3(14f, 2f, 15f));
            CreateEnemy("Krab_A", EnemyKind.Krab, dark, red, enemyProjectile, new Vector3(-14f, 2f, 20f));
            CreateEnemy("Hornet_A", EnemyKind.Hornet, orange, red, enemyProjectile, new Vector3(8f, 5f, 27f));
            CreateEnemy("Megatank_Boss", EnemyKind.Megatank, dark, red, enemyProjectile, new Vector3(0f, 2.2f, 41f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath);

            Debug.Log("Code Lyoko vertical slice generated at " + ScenePath);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(GeneratedFolder);
            Directory.CreateDirectory(PrefabFolder);
            Directory.CreateDirectory(MaterialFolder);
            Directory.CreateDirectory("Assets/Levels");
        }

        private static void EnsureTag(string tag)
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

        private static Material Material(string name, Color color, float emission)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetColor("_BaseColor", color);

            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Projectile BuildProjectilePrefab(string name, Material material, bool hostile)
        {
            string path = PrefabFolder + "/" + name + ".prefab";
            Projectile existing = AssetDatabase.LoadAssetAtPath<Projectile>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            root.name = name;
            root.transform.localScale = hostile ? Vector3.one * 0.28f : Vector3.one * 0.22f;
            root.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = root.GetComponent<Collider>();
            collider.isTrigger = true;
            root.AddComponent<Rigidbody>().isKinematic = true;
            Projectile projectile = root.AddComponent<Projectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<Projectile>();
        }

        private static VehicleRig BuildVehiclePrefab(string name, VehicleStyle style, Material material, PrimitiveType primitive)
        {
            string path = PrefabFolder + "/" + name + ".prefab";
            VehicleRig existing = AssetDatabase.LoadAssetAtPath<VehicleRig>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject root = new GameObject(name);
            VehicleRig rig = root.AddComponent<VehicleRig>();
            rig.Configure(style);

            GameObject body = GameObject.CreatePrimitive(primitive);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = style == VehicleStyle.Overbike ? new Vector3(0.18f, 0.9f, 0.9f) : new Vector3(1.45f, 0.15f, 2.1f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            if (style == VehicleStyle.Overbike)
            {
                body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<VehicleRig>();
        }

        private static Camera CreateCamera(LockOnSystem lockOn)
        {
            GameObject cameraObject = new GameObject("ThirdPersonCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 7f, -20f);
            cameraObject.AddComponent<AudioListener>();
            ThirdPersonCamera follow = cameraObject.AddComponent<ThirdPersonCamera>();
            SetPrivate(follow, "lockOn", lockOn);
            return camera;
        }

        private static void CreateLighting()
        {
            GameObject sun = new GameObject("VirtualSun");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(0.85f, 1f, 0.95f);
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void CreateLevelGeometry(Material forest, Material dark, Material cyan)
        {
            CreatePlatform("StartPlateau", new Vector3(0f, 0f, -12f), new Vector3(24f, 1f, 20f), forest);
            CreatePlatform("BridgeDataPath", new Vector3(0f, 0f, 8f), new Vector3(9f, 1f, 24f), forest);
            CreatePlatform("CombatPlateau", new Vector3(0f, 0f, 28f), new Vector3(30f, 1f, 24f), forest);
            CreatePlatform("TowerPlateau", new Vector3(0f, 0f, 48f), new Vector3(22f, 1f, 18f), forest);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 11f, 1.2f, 48f + Mathf.Sin(angle) * 8f);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = "DataTree_" + i;
                pillar.transform.position = pos;
                pillar.transform.localScale = new Vector3(0.45f, 2.4f + (i % 3), 0.45f);
                pillar.GetComponent<Renderer>().sharedMaterial = i % 2 == 0 ? dark : cyan;
            }

            GameObject sea = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sea.name = "DigitalSea";
            sea.transform.position = new Vector3(0f, -8.2f, 20f);
            sea.transform.localScale = new Vector3(120f, 0.35f, 120f);
            sea.GetComponent<Renderer>().sharedMaterial = cyan;
            Object.DestroyImmediate(sea.GetComponent<Collider>());
        }

        private static GameObject CreatePlatform(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.position = position;
            platform.transform.localScale = scale;
            platform.GetComponent<Renderer>().sharedMaterial = material;
            return platform;
        }

        private static PlayerController CreatePlayer(string name, LyokoCharacter id, AttackStyle style, VehicleStyle vehicleStyle, Material material, VehicleRig vehicle, Projectile projectile, CharacterSwitcher switcher, LockOnSystem lockOn, Vector3 position)
        {
            GameObject root = new GameObject(name);
            root.tag = "Player";
            root.transform.position = position;
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.center = Vector3.up;
            root.AddComponent<Health>().SetMaxHealth(130f);
            PlayerCombat combat = root.AddComponent<PlayerCombat>();
            PlayerController player = root.AddComponent<PlayerController>();

            SetPrivate(player, "character", id);
            SetPrivate(player, "vehicleStyle", vehicleStyle);
            SetPrivate(player, "vehiclePrefab", vehicle);
            combat.Configure(style, projectile, lockOn);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "PrototypeBody";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.up;
            body.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visor.name = "LyokoFace";
            visor.transform.SetParent(root.transform);
            visor.transform.localPosition = new Vector3(0f, 1.72f, 0.36f);
            visor.transform.localScale = new Vector3(0.36f, 0.18f, 0.08f);
            visor.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(visor.GetComponent<Collider>());

            return player;
        }

        private static AelitaCompanion CreateAelita(Material material, CharacterSwitcher switcher, Vector3 position)
        {
            GameObject root = new GameObject("Aelita_Companion");
            root.transform.position = position;
            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.34f;
            controller.center = Vector3.up * 0.9f;
            root.AddComponent<Health>().SetMaxHealth(100f);
            AelitaCompanion companion = root.AddComponent<AelitaCompanion>();
            SetPrivate(companion, "switcher", switcher);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "PrototypeBody";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.up * 0.9f;
            body.transform.localScale = new Vector3(0.65f, 0.9f, 0.65f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            return companion;
        }

        private static TowerObjective CreateTower(Material cyan, Material red, AelitaCompanion aelita)
        {
            GameObject root = new GameObject("ActivatedTower");
            root.transform.position = new Vector3(0f, 3f, 50f);

            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = "TowerColumn";
            cylinder.transform.SetParent(root.transform);
            cylinder.transform.localPosition = Vector3.zero;
            cylinder.transform.localScale = new Vector3(2.8f, 3f, 2.8f);
            cylinder.GetComponent<Renderer>().sharedMaterial = red;

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "TowerHalo";
            ring.transform.SetParent(root.transform);
            ring.transform.localPosition = Vector3.up * 3.2f;
            ring.transform.localScale = new Vector3(3.8f, 0.08f, 3.8f);
            ring.GetComponent<Renderer>().sharedMaterial = cyan;

            TowerObjective tower = root.AddComponent<TowerObjective>();
            tower.SetAelita(aelita);
            SetPrivate(tower, "towerRenderer", cylinder.GetComponent<Renderer>());
            return tower;
        }

        private static EnemyAI CreateEnemy(string name, EnemyKind kind, Material bodyMaterial, Material eyeMaterial, Projectile projectile, Vector3 position)
        {
            GameObject root = new GameObject(name);
            root.tag = "Enemy";
            root.transform.position = position;
            root.AddComponent<Health>();
            EnemyAI ai = root.AddComponent<EnemyAI>();
            ai.Configure(kind, projectile);

            GameObject body = GameObject.CreatePrimitive(kind == EnemyKind.Megatank ? PrimitiveType.Sphere : PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = Vector3.up;
            body.transform.localScale = EnemyScale(kind);
            body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            GameObject colliderObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            colliderObject.name = "HitCollider";
            colliderObject.transform.SetParent(root.transform);
            colliderObject.transform.localPosition = Vector3.up;
            colliderObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            colliderObject.tag = "Enemy";
            colliderObject.GetComponent<Renderer>().enabled = false;

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "XanaEyeWeakPoint";
            eye.tag = "Enemy";
            eye.transform.SetParent(root.transform);
            eye.transform.localPosition = new Vector3(0f, 1.15f, -0.72f);
            eye.transform.localScale = Vector3.one * (kind == EnemyKind.Megatank ? 0.62f : 0.34f);
            eye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
            eye.GetComponent<Collider>().isTrigger = true;
            eye.AddComponent<WeakPoint>();

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(root.transform);
            firePoint.transform.localPosition = new Vector3(0f, 1.2f, -1f);
            SetPrivate(ai, "firePoint", firePoint.transform);
            SetPrivate(ai, "aimPoint", eye.transform);

            if (kind == EnemyKind.Krab || kind == EnemyKind.Kankrelat)
            {
                for (int i = 0; i < 4; i++)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.name = "Leg_" + i;
                    leg.transform.SetParent(root.transform);
                    leg.transform.localPosition = new Vector3(i < 2 ? -0.75f : 0.75f, 0.45f, i % 2 == 0 ? -0.55f : 0.55f);
                    leg.transform.localRotation = Quaternion.Euler(25f, 0f, i < 2 ? 28f : -28f);
                    leg.transform.localScale = new Vector3(0.12f, 0.75f, 0.12f);
                    leg.GetComponent<Renderer>().sharedMaterial = bodyMaterial;
                    Object.DestroyImmediate(leg.GetComponent<Collider>());
                }
            }

            return ai;
        }

        private static Vector3 EnemyScale(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Blok:
                    return new Vector3(1.5f, 1.5f, 1.5f);
                case EnemyKind.Krab:
                    return new Vector3(2.2f, 1.1f, 1.7f);
                case EnemyKind.Hornet:
                    return new Vector3(1.2f, 0.55f, 2.1f);
                case EnemyKind.Megatank:
                    return new Vector3(2.8f, 2.8f, 2.8f);
                default:
                    return new Vector3(1.2f, 0.65f, 1f);
            }
        }

        private static void SetPrivate<T>(Object target, string fieldName, T value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                EditorUtility.SetDirty(target);
            }
        }
    }
}
