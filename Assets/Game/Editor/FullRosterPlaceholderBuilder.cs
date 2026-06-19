using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    public static class FullRosterPlaceholderBuilder
    {
        private const string RootFolder = "Assets/Game/Generated/FullRosterPlaceholders";
        private const string CharacterFolder = RootFolder + "/Characters";
        private const string EnemyFolder = RootFolder + "/Enemies";
        private const string VehicleFolder = RootFolder + "/Vehicles";
        private const string MaterialFolder = RootFolder + "/Materials";

        [MenuItem("Tools/Code Lyoko Fan Game/Create Full Roster Placeholder Prefabs")]
        public static void CreateFullRosterPlaceholders()
        {
            Directory.CreateDirectory(CharacterFolder);
            Directory.CreateDirectory(EnemyFolder);
            Directory.CreateDirectory(VehicleFolder);
            Directory.CreateDirectory(MaterialFolder);
            EnsureTag("Enemy");

            Material purple = Material("OddPurple", new Color(0.48f, 0.16f, 0.95f), 0.4f);
            Material green = Material("UlrichGreen", new Color(0.06f, 0.42f, 0.18f), 0.35f);
            Material black = Material("YumiBlack", new Color(0.02f, 0.02f, 0.025f), 0.2f);
            Material pink = Material("AelitaPink", new Color(1f, 0.42f, 0.78f), 0.65f);
            Material blue = Material("JeremieBlue", new Color(0.12f, 0.42f, 0.9f), 0.25f);
            Material grey = Material("LyokoGrey", new Color(0.24f, 0.28f, 0.3f), 0.2f);
            Material red = Material("XanaRed", new Color(1f, 0.04f, 0.03f), 1.8f);
            Material orange = Material("XanaOrange", new Color(1f, 0.42f, 0.04f), 1.1f);
            Material cyan = Material("LyokoCyan", new Color(0.1f, 0.85f, 1f), 1.4f);

            Character("Odd", purple, true, false);
            Character("Ulrich", green, false, true);
            Character("Yumi", black, false, false);
            Character("Aelita", pink, true, false);
            Character("Jeremie", blue, false, false);
            Character("William", grey, false, true);
            Character("XanaWilliam", red, false, true);

            Enemy("Kankrelat", EnemyKind.Kankrelat, orange, red);
            Enemy("Blok", EnemyKind.Blok, grey, red);
            Enemy("Krab", EnemyKind.Krab, grey, red);
            Enemy("Hornet", EnemyKind.Hornet, orange, red);
            Enemy("Megatank", EnemyKind.Megatank, grey, red);
            Enemy("Tarantula", EnemyKind.Tarantula, grey, red);
            Enemy("Creeper", EnemyKind.Creeper, orange, red);
            Enemy("Manta", EnemyKind.Manta, grey, red);
            Enemy("BlackManta", EnemyKind.BlackManta, red, red);
            Enemy("Kongre", EnemyKind.Kongre, orange, red);
            Enemy("Shark", EnemyKind.Shark, grey, red);
            Enemy("Kalamar", EnemyKind.Kalamar, orange, red);
            Enemy("Guardian", EnemyKind.Guardian, orange, red);
            Enemy("Scyphozoa", EnemyKind.Scyphozoa, cyan, red);
            Enemy("Kolossus", EnemyKind.Kolossus, orange, red);
            Enemy("Specter", EnemyKind.Specter, red, red);
            Enemy("PolymorphicClone", EnemyKind.PolymorphicClone, red, red);
            Enemy("Zombies", EnemyKind.Zombies, grey, red);
            Enemy("XanaWilliamEnemy", EnemyKind.XanaWilliam, red, red);

            Vehicle("Overboard", VehicleStyle.Overboard, purple);
            Vehicle("Overbike", VehicleStyle.Overbike, green);
            Vehicle("Overwing", VehicleStyle.Overwing, black);
            Vehicle("Skidbladnir", VehicleStyle.Skidbladnir, cyan);
            Vehicle("NavSkid", VehicleStyle.NavSkid, cyan);
            Vehicle("TransportOrb", VehicleStyle.TransportOrb, cyan);
            Vehicle("BlackMantaVehicle", VehicleStyle.BlackManta, red);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Full roster placeholder prefabs created in " + RootFolder);
        }

        private static void Character(string name, Material material, bool lightFrame, bool sword)
        {
            GameObject root = new GameObject(name);
            GameObject body = Primitive("Body", PrimitiveType.Capsule, root.transform, Vector3.up, new Vector3(0.75f, 1f, 0.75f), material);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Primitive("HeadMarker", PrimitiveType.Sphere, root.transform, new Vector3(0f, 1.82f, 0.08f), new Vector3(0.42f, 0.34f, 0.42f), material);

            if (lightFrame)
            {
                Primitive("LightFrame", PrimitiveType.Cube, root.transform, new Vector3(0f, 1.1f, 0.46f), new Vector3(0.55f, 0.08f, 0.08f), material);
            }

            if (sword)
            {
                Primitive("WeaponProxy", PrimitiveType.Cube, root.transform, new Vector3(0.55f, 1f, 0.15f), new Vector3(0.08f, 1.35f, 0.08f), material);
            }

            SavePrefab(root, CharacterFolder + "/" + name + ".prefab");
        }

        private static void Enemy(string name, EnemyKind kind, Material bodyMaterial, Material eyeMaterial)
        {
            GameObject root = new GameObject(name);
            root.tag = "Enemy";
            root.AddComponent<Health>();
            EnemyAI enemy = root.AddComponent<EnemyAI>();
            enemy.Configure(kind, null);

            Vector3 scale = kind == EnemyKind.Kolossus ? new Vector3(4f, 6f, 2f) : EnemyScale(kind);
            Primitive("Body", kind == EnemyKind.Megatank ? PrimitiveType.Sphere : PrimitiveType.Cube, root.transform, Vector3.up, scale, bodyMaterial);

            GameObject eye = Primitive("XanaEyeWeakPoint", PrimitiveType.Sphere, root.transform, new Vector3(0f, scale.y * 0.65f + 0.6f, -scale.z * 0.45f), Vector3.one * 0.36f, eyeMaterial);
            Collider eyeCollider = eye.GetComponent<Collider>();
            if (eyeCollider != null)
            {
                eyeCollider.isTrigger = true;
            }
            eye.AddComponent<WeakPoint>();

            if (kind == EnemyKind.Krab || kind == EnemyKind.Kankrelat || kind == EnemyKind.Tarantula)
            {
                for (int i = 0; i < 6; i++)
                {
                    float side = i < 3 ? -1f : 1f;
                    float z = -0.8f + (i % 3) * 0.8f;
                    Primitive("Leg_" + i, PrimitiveType.Cylinder, root.transform, new Vector3(side * 0.85f, 0.45f, z), new Vector3(0.1f, 0.7f, 0.1f), bodyMaterial);
                }
            }

            if (kind == EnemyKind.Scyphozoa || kind == EnemyKind.Kalamar)
            {
                for (int i = 0; i < 5; i++)
                {
                    Primitive("Tentacle_" + i, PrimitiveType.Cylinder, root.transform, new Vector3(-0.8f + i * 0.4f, 0.2f, -0.35f), new Vector3(0.08f, 1.1f, 0.08f), bodyMaterial);
                }
            }

            SavePrefab(root, EnemyFolder + "/" + name + ".prefab");
        }

        private static void Vehicle(string name, VehicleStyle style, Material material)
        {
            GameObject root = new GameObject(name);
            VehicleRig rig = root.AddComponent<VehicleRig>();
            rig.Configure(style);

            Primitive("Body", style == VehicleStyle.Overbike ? PrimitiveType.Cylinder : PrimitiveType.Cube, root.transform, Vector3.zero, VehicleScale(style), material);

            if (style == VehicleStyle.Skidbladnir)
            {
                Primitive("Nose", PrimitiveType.Sphere, root.transform, new Vector3(0f, 0f, 2.2f), new Vector3(1.3f, 0.8f, 1.3f), material);
            }

            SavePrefab(root, VehicleFolder + "/" + name + ".prefab");
        }

        private static GameObject Primitive(string name, PrimitiveType primitive, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(primitive);
            obj.name = name;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = localScale;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            return obj;
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
                case EnemyKind.Manta:
                case EnemyKind.BlackManta:
                    return new Vector3(3f, 0.38f, 2.2f);
                case EnemyKind.Kongre:
                case EnemyKind.Shark:
                    return new Vector3(1f, 0.8f, 3.2f);
                case EnemyKind.Scyphozoa:
                    return new Vector3(1.7f, 1.8f, 1.7f);
                default:
                    return new Vector3(1.2f, 0.9f, 1.2f);
            }
        }

        private static Vector3 VehicleScale(VehicleStyle style)
        {
            switch (style)
            {
                case VehicleStyle.Overbike:
                    return new Vector3(0.22f, 1f, 1f);
                case VehicleStyle.Overwing:
                    return new Vector3(2.2f, 0.18f, 1.6f);
                case VehicleStyle.Skidbladnir:
                    return new Vector3(2.4f, 1f, 4.2f);
                case VehicleStyle.NavSkid:
                    return new Vector3(0.85f, 0.55f, 1.8f);
                case VehicleStyle.TransportOrb:
                    return Vector3.one * 1.8f;
                case VehicleStyle.BlackManta:
                    return new Vector3(3f, 0.25f, 2f);
                default:
                    return new Vector3(1.45f, 0.15f, 2.1f);
            }
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

        private static void SavePrefab(GameObject root, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
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
    }
}
