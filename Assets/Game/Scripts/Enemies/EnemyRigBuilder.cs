using UnityEngine;

namespace CodeLyokoFanGame
{
    // Monster bodies shared by the scene generators (edit mode, with materials) and runtime spawns (play mode).
    // Pivot at the feet for ground units. Callers finish with EnemyAI.Configure(definition, projectile).
    // F09: the primitives keep their colliders, but show faceted low-poly meshes with a toon outline,
    // the eye of X.A.N.A. symbol, and a MonsterAnimator for legs and wings.
    public static class EnemyRigBuilder
    {
        private const float Outline = 1.2f;
        private static readonly Color WingColor = new Color(0.85f, 0.9f, 0.95f);

        public static EnemyAI Build(string name, EnemyKind kind, Vector3 position, Material bodyMaterial, Material eyeMaterial)
        {
            GameObject root = new GameObject(name);
            root.tag = "Enemy";
            root.transform.position = position;
            root.AddComponent<Health>();
            EnemyAI enemy = root.AddComponent<EnemyAI>();

            Color bodyColor = kind == EnemyKind.Kankrelat || kind == EnemyKind.Hornet ? EnemyVisuals.XanaOrange : EnemyVisuals.DarkMetal;
            Transform body;
            GameObject eye;
            Transform firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(root.transform, false);

            switch (kind)
            {
                case EnemyKind.Kankrelat:
                    body = Part(PrimitiveType.Cube, "Body", root.transform, new Vector3(0f, 0.6f, 0f), new Vector3(1.2f, 0.55f, 1f), bodyMaterial, bodyColor, false, BodyGem());
                    Legs(root.transform, 4, 0.7f, 0.5f, new Vector3(0.1f, 0.35f, 0.1f), 0.3f, bodyMaterial, bodyColor, false, 0f);
                    HitCapsule(root.transform, new Vector3(0f, 0.8f, 0f), 0.65f, 1.6f, 1);
                    eye = Eye(root.transform, new Vector3(0f, 1f, 0.3f), 0.34f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 1f, 0.75f);
                    break;
                case EnemyKind.Blok:
                    body = Part(PrimitiveType.Cube, "Body", root.transform, new Vector3(0f, 1f, 0f), Vector3.one * 1.5f, bodyMaterial, bodyColor, true, BlockBody());
                    Legs(root.transform, 4, 0.55f, 0.55f, new Vector3(0.14f, 0.15f, 0.14f), 0.12f, bodyMaterial, bodyColor, false, 0f);
                    eye = Eye(root.transform, new Vector3(0f, 1f, 0.82f), 0.42f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 1f, 1.15f);
                    break;
                case EnemyKind.Krab:
                    body = Part(PrimitiveType.Cube, "Body", root.transform, new Vector3(0f, 2.1f, 0f), new Vector3(2.2f, 0.9f, 1.7f), bodyMaterial, bodyColor, true, BodyGem());
                    Legs(root.transform, 4, 0.95f, 0.65f, new Vector3(0.22f, 1f, 0.22f), 1f, bodyMaterial, bodyColor, true, 12f);
                    eye = Eye(root.transform, new Vector3(0f, 2.65f, 0.25f), 0.46f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 2.7f, 0.9f);
                    break;
                case EnemyKind.Hornet:
                    body = Part(PrimitiveType.Cube, "Body", root.transform, Vector3.zero, new Vector3(1f, 0.5f, 1.8f), bodyMaterial, bodyColor, false, BodyGem());
                    Part(PrimitiveType.Cube, "Wing_L", root.transform, new Vector3(-0.9f, 0.25f, 0f), new Vector3(1.4f, 0.04f, 0.6f), null, WingColor, false, BodyGem());
                    Part(PrimitiveType.Cube, "Wing_R", root.transform, new Vector3(0.9f, 0.25f, 0f), new Vector3(1.4f, 0.04f, 0.6f), null, WingColor, false, BodyGem());
                    HitCapsule(root.transform, Vector3.zero, 0.7f, 2.2f, 2);
                    eye = Eye(root.transform, new Vector3(0f, 0.2f, 0.95f), 0.32f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 0.05f, 1.25f);
                    break;
                case EnemyKind.Megatank:
                    body = Part(PrimitiveType.Sphere, "Body", root.transform, new Vector3(0f, 1.4f, 0f), Vector3.one * 2.8f, bodyMaterial, bodyColor, true, MegatankBody());
                    body.gameObject.AddComponent<Hurtbox>();
                    GameObject iris = Part(PrimitiveType.Cylinder, "Iris", root.transform, new Vector3(0f, 1.4f, 1.3f), new Vector3(1.7f, 0.05f, 1.7f), eyeMaterial, EnemyVisuals.XanaRed, false).gameObject;
                    iris.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    iris.GetComponent<Renderer>().enabled = false;
                    eye = Eye(root.transform, new Vector3(0f, 1.4f, 1.45f), 0.62f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 1.4f, 1.9f);
                    break;
                default:
                    body = Part(PrimitiveType.Cube, "Body", root.transform, Vector3.up, new Vector3(1.2f, 0.9f, 1.2f), bodyMaterial, bodyColor, false, BodyGem());
                    HitCapsule(root.transform, Vector3.up, 0.6f, 2.4f, 1);
                    eye = Eye(root.transform, new Vector3(0f, 1.25f, 0.7f), 0.34f, eyeMaterial);
                    firePoint.localPosition = new Vector3(0f, 1.25f, 1f);
                    break;
            }

            enemy.SetAnchors(firePoint, eye.transform);
            enemy.SetBodyVisual(body);
            root.AddComponent<MonsterAnimator>();
            GameLayers.SetRecursively(root, GameLayers.Enemy);
            return enemy;
        }

        private static Transform Part(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material, Color color, bool keepCollider, Mesh mesh = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.tag = "Enemy";
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = scale;
            if (!keepCollider)
            {
                EnemyVisuals.RemoveCollider(part);
            }

            if (mesh != null)
            {
                part.GetComponent<MeshFilter>().sharedMesh = mesh;
            }
            else
            {
                LowPolyMesh.Facet(part, type);
            }

            // Only the eye and the Megatank iris glow; bodies stay flat.
            EnemyVisuals.Paint(part, material, color, Outline, color == EnemyVisuals.XanaRed);
            return part.transform;
        }

        // Legs at the four corners. Breakable legs keep their collider and get their own Health.
        private static void Legs(Transform root, int count, float halfWidth, float halfDepth, Vector3 scale, float height, Material material, Color color, bool breakable, float splay)
        {
            for (int i = 0; i < count; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float front = i < 2 ? 1f : -1f;
                Transform leg = Part(PrimitiveType.Cylinder, "Leg_" + i, root, new Vector3(side * halfWidth, height, front * halfDepth), scale, material, color, breakable, LegMesh());
                leg.localRotation = Quaternion.Euler(front * splay, 0f, -side * (splay + 14f));
                if (breakable)
                {
                    leg.gameObject.AddComponent<Health>();
                    leg.gameObject.AddComponent<EnemyPart>().Configure("Leg_" + i, 25f);
                }
            }
        }

        private static void HitCapsule(Transform root, Vector3 center, float radius, float height, int direction)
        {
            GameObject hit = new GameObject("HitCollider");
            hit.tag = "Enemy";
            hit.transform.SetParent(root, false);
            CapsuleCollider capsule = hit.AddComponent<CapsuleCollider>();
            capsule.center = center;
            capsule.radius = radius;
            capsule.height = height;
            capsule.direction = direction;
        }

        private static GameObject Eye(Transform root, Vector3 localPosition, float size, Material material)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "XanaEyeWeakPoint";
            eye.tag = "Enemy";
            eye.transform.SetParent(root, false);
            eye.transform.localPosition = localPosition;
            eye.transform.localScale = Vector3.one * size;
            eye.GetComponent<Collider>().isTrigger = true;
            eye.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.XanaEye();
            EnemyVisuals.Paint(eye, material, EnemyVisuals.XanaRed);
            eye.AddComponent<WeakPoint>();
            return eye;
        }

        // Unit-sized meshes that replace the primitives (same size, so the colliders still match).
        private static Mesh BodyGem()
        {
            return LowPolyMesh.Gem(0.5f, 1);
        }

        private static Mesh BlockBody()
        {
            return LowPolyMesh.Get("MonsterBlockBody", b =>
            {
                b.Box(Matrix4x4.Translate(new Vector3(0f, -0.05f, 0f)), new Vector3(1f, 0.9f, 1f), new Vector2(0.9f, 0.9f));
                b.Box(Matrix4x4.Translate(new Vector3(0f, 0.45f, 0f)), new Vector3(0.8f, 0.1f, 0.8f));
            });
        }

        private static Mesh MegatankBody()
        {
            return LowPolyMesh.Get("MegatankBody", b =>
            {
                b.Gem(Matrix4x4.identity, 0.5f, 2);
                b.Ring(Matrix4x4.identity, 0.49f, 0.04f, 20, 4);
            });
        }

        // Thick at the hip, thin at the foot; centred like the Unity cylinder (height 2).
        private static Mesh LegMesh()
        {
            return LowPolyMesh.Get("MonsterLeg", b => b.Prism(Matrix4x4.Translate(new Vector3(0f, -1f, 0f)), 6, 0.3f, 0.5f, 2f));
        }
    }
}
