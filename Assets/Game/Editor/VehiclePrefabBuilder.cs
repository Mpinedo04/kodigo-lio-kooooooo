using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // Placeholder vehicle prefabs (F08): VehicleController + Health + a "Visual" made of primitives.
    // The pivot sits at the rider's feet; the hull hangs below by the definition's seat height.
    public static class VehiclePrefabBuilder
    {
        private const string PrefabFolder = "Assets/Game/Generated/Prefabs";

        public static VehicleController Build(string name, VehicleStyle style, Material material)
        {
            string path = PrefabFolder + "/" + name + ".prefab";
            VehicleDefinition definition = VehicleDataBuilder.Ensure(style, name);
            Material glow = CodeLyokoSceneBuilder.Material("Lyoko_Cyan_Emission", new Color(0.1f, 0.85f, 1f), 1.7f);

            GameObject root = new GameObject(name);
            Health health = root.AddComponent<Health>();
            health.SetTeam(Team.Player);
            health.SetDestroyOnDeath(false);
            health.SetMaxHealth(definition.maxHealth);
            health.SetHitInvulnerability(definition.hitInvulnerability);
            VehicleController vehicle = root.AddComponent<VehicleController>();
            vehicle.Configure(definition);

            Transform visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);

            switch (style)
            {
                case VehicleStyle.Overbike:
                    BuildOverbike(visual, material, glow, definition.seatHeight);
                    break;
                case VehicleStyle.Overwing:
                    BuildOverwing(visual, material, glow, definition.seatHeight);
                    break;
                default:
                    BuildOverboard(visual, material, glow);
                    break;
            }

            PassengerSeat seat = null;
            if (definition.canCarryPassenger)
            {
                GameObject seatObject = new GameObject("PassengerSeat");
                seatObject.transform.SetParent(root.transform, false);
                seatObject.transform.localPosition = definition.passengerSeatOffset;
                seat = seatObject.AddComponent<PassengerSeat>();
                seat.Configure(definition);
                seat.SetProjectile(AssetDatabase.LoadAssetAtPath<Projectile>(PrefabFolder + "/PlayerEnergyDart.prefab"));
            }

            CodeLyokoSceneBuilder.SetPrivate(vehicle, "visual", visual);
            CodeLyokoSceneBuilder.SetPrivate(vehicle, "passengerSeat", seat);
            GameLayers.SetRecursively(root, GameLayers.Player);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<VehicleController>();
        }

        // Odd's purple board with glowing rails.
        private static void BuildOverboard(Transform visual, Material body, Material glow)
        {
            Part("Deck", PrimitiveType.Cube, visual, new Vector3(0f, -0.07f, 0f), new Vector3(0.9f, 0.12f, 1.9f), Vector3.zero, body);
            Part("Nose", PrimitiveType.Cube, visual, new Vector3(0f, -0.04f, 1.05f), new Vector3(0.6f, 0.1f, 0.45f), new Vector3(-12f, 0f, 0f), body);
            Part("Rail_L", PrimitiveType.Cube, visual, new Vector3(-0.46f, -0.07f, 0f), new Vector3(0.05f, 0.06f, 1.7f), Vector3.zero, glow);
            Part("Rail_R", PrimitiveType.Cube, visual, new Vector3(0.46f, -0.07f, 0f), new Vector3(0.05f, 0.06f, 1.7f), Vector3.zero, glow);
            Part("UnderGlow", PrimitiveType.Cube, visual, new Vector3(0f, -0.15f, -0.1f), new Vector3(0.5f, 0.03f, 1.2f), Vector3.zero, glow);
        }

        // Ulrich's one-wheel bike: a big wheel under the seat and handlebars in front.
        private static void BuildOverbike(Transform visual, Material body, Material glow, float seatHeight)
        {
            float radius = seatHeight * 0.5f;
            Part("Wheel", PrimitiveType.Cylinder, visual, new Vector3(0f, -radius, 0f), new Vector3(radius * 2f, 0.14f, radius * 2f), new Vector3(0f, 0f, 90f), body);
            Part("Rim", PrimitiveType.Cylinder, visual, new Vector3(0f, -radius, 0f), new Vector3(radius * 1.5f, 0.16f, radius * 1.5f), new Vector3(0f, 0f, 90f), glow);
            Part("Seat", PrimitiveType.Cube, visual, new Vector3(0f, -0.04f, -0.05f), new Vector3(0.38f, 0.12f, 0.95f), Vector3.zero, body);
            Part("Fairing", PrimitiveType.Cube, visual, new Vector3(0f, 0.2f, 0.5f), new Vector3(0.45f, 0.4f, 0.25f), new Vector3(-20f, 0f, 0f), body);
            Part("Handlebar", PrimitiveType.Cube, visual, new Vector3(0f, 0.5f, 0.55f), new Vector3(0.85f, 0.06f, 0.06f), Vector3.zero, glow);
        }

        // Yumi's flying scooter: long hull, short wings and room for a passenger at the back.
        private static void BuildOverwing(Transform visual, Material body, Material glow, float seatHeight)
        {
            float hullY = -seatHeight * 0.45f;
            Part("Hull", PrimitiveType.Cube, visual, new Vector3(0f, hullY, -0.1f), new Vector3(0.9f, 0.35f, 2.5f), Vector3.zero, body);
            Part("Wings", PrimitiveType.Cube, visual, new Vector3(0f, hullY - 0.05f, 0.1f), new Vector3(2.8f, 0.06f, 0.8f), Vector3.zero, body);
            Part("WingGlow", PrimitiveType.Cube, visual, new Vector3(0f, hullY - 0.1f, 0.1f), new Vector3(2.6f, 0.03f, 0.3f), Vector3.zero, glow);
            Part("TailFin", PrimitiveType.Cube, visual, new Vector3(0f, hullY + 0.35f, -1.25f), new Vector3(0.06f, 0.5f, 0.5f), Vector3.zero, body);
            Part("Handlebar", PrimitiveType.Cube, visual, new Vector3(0f, 0.55f, 0.75f), new Vector3(0.8f, 0.06f, 0.06f), Vector3.zero, glow);
            Part("Column", PrimitiveType.Cube, visual, new Vector3(0f, 0.2f, 0.85f), new Vector3(0.08f, 0.7f, 0.08f), new Vector3(-15f, 0f, 0f), body);
        }

        private static void Part(string name, PrimitiveType primitive, Transform parent, Vector3 position, Vector3 scale, Vector3 euler, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());

            // F09: faceted meshes (same size as the primitives) with the toon shader and an outline.
            Mesh mesh = primitive == PrimitiveType.Cylinder
                ? LowPolyMesh.Get("UnitCylinder16", b => b.Prism(Matrix4x4.Translate(new Vector3(0f, -1f, 0f)), 16, 0.5f, 0.5f, 2f))
                : primitive == PrimitiveType.Cube ? LowPolyMesh.Box(Vector3.one, new Vector2(0.92f, 0.92f)) : null;
            if (mesh != null)
            {
                part.GetComponent<MeshFilter>().sharedMesh = mesh;
            }

            EnemyVisuals.Paint(part, material, Color.white, 1f);
        }
    }
}
