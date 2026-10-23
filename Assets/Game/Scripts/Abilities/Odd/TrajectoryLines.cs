using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Lines for Future Vision: where each enemy shot will go and where each monster is aiming.
    public sealed class TrajectoryLines : MonoBehaviour
    {
        private const float ShotLookAhead = 1.5f;
        private const float AimLength = 12f;

        private static readonly Color ShotColor = new Color(1f, 0.2f, 0.15f, 0.9f);
        private static readonly Color AimColor = new Color(1f, 0.6f, 0.1f, 0.35f);

        private readonly List<LineRenderer> lines = new List<LineRenderer>();
        private Material material;

        public static TrajectoryLines Create()
        {
            return new GameObject("FutureVisionLines").AddComponent<TrajectoryLines>();
        }

        private void Awake()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }

            material = shader != null ? new Material(shader) : null;
        }

        private void LateUpdate()
        {
            int used = 0;

            IReadOnlyList<Projectile> shots = Projectile.All;
            for (int i = 0; i < shots.Count; i++)
            {
                if (shots[i].Team != Team.Enemy)
                {
                    continue;
                }

                Vector3 from = shots[i].transform.position;
                Draw(used++, from, from + shots[i].Velocity * ShotLookAhead, ShotColor, 0.09f);
            }

            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (!AbilityMath.IsEnemy(all[i]))
                {
                    continue;
                }

                Transform monster = all[i].transform;
                Vector3 from = monster.position + Vector3.up;
                Draw(used++, from, from + monster.forward * AimLength, AimColor, 0.04f);
            }

            for (int i = used; i < lines.Count; i++)
            {
                lines[i].enabled = false;
            }
        }

        private void Draw(int index, Vector3 from, Vector3 to, Color color, float width)
        {
            while (lines.Count <= index)
            {
                GameObject child = new GameObject("Line");
                child.transform.SetParent(transform, false);
                LineRenderer created = child.AddComponent<LineRenderer>();
                created.positionCount = 2;
                created.useWorldSpace = true;
                if (material != null)
                {
                    created.sharedMaterial = material;
                }

                lines.Add(created);
            }

            LineRenderer line = lines[index];
            line.enabled = true;
            line.startWidth = width;
            line.endWidth = width * 0.4f;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0f);
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }

        private void OnDestroy()
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}
