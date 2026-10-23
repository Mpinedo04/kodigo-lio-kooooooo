using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Builds faceted low-poly meshes out of simple convex pieces (F09). Every triangle is flat shaded;
    // UV3 keeps smoothed normals so the toon outline does not split on hard edges.
    // Triangles are turned to face away from the centre of the piece they belong to, so winding never matters.
    public sealed class MeshBuilder
    {
        private static readonly int[] IcoFaces =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };

        private readonly List<Vector3> corners = new List<Vector3>();
        private readonly List<int> pieces = new List<int>();
        private int piece;

        public int TriangleCount => corners.Count / 3;

        public void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 inside)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-12f)
            {
                return;
            }

            if (Vector3.Dot(normal, (a + b + c) / 3f - inside) < 0f)
            {
                Vector3 swap = b;
                b = c;
                c = swap;
            }

            corners.Add(a);
            corners.Add(b);
            corners.Add(c);
            pieces.Add(piece);
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 inside)
        {
            Triangle(a, b, c, inside);
            Triangle(a, c, d, inside);
        }

        // Tapered cylinder along +Y with its pivot at the bottom centre. depth squashes it front to back.
        public MeshBuilder Prism(Matrix4x4 m, int sides, float bottomRadius, float topRadius, float height, float depth = 1f)
        {
            piece++;
            Vector3 inside = m.MultiplyPoint3x4(new Vector3(0f, height * 0.5f, 0f));
            Vector3[] bottom = new Vector3[sides];
            Vector3[] top = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / sides;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle) * depth;
                bottom[i] = m.MultiplyPoint3x4(new Vector3(x * bottomRadius, 0f, z * bottomRadius));
                top[i] = m.MultiplyPoint3x4(new Vector3(x * topRadius, height, z * topRadius));
            }

            Vector3 bottomCenter = m.MultiplyPoint3x4(Vector3.zero);
            Vector3 topCenter = m.MultiplyPoint3x4(new Vector3(0f, height, 0f));
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                if (topRadius > 0.0001f)
                {
                    Quad(bottom[i], bottom[next], top[next], top[i], inside);
                    Triangle(topCenter, top[i], top[next], inside);
                }
                else
                {
                    Triangle(bottom[i], bottom[next], topCenter, inside);
                }

                if (bottomRadius > 0.0001f)
                {
                    Triangle(bottomCenter, bottom[next], bottom[i], inside);
                }
            }

            return this;
        }

        // Faceted sphere (icosphere) of the given radius around the matrix origin.
        public MeshBuilder Gem(Matrix4x4 m, float radius, int subdivisions = 1)
        {
            piece++;
            float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            List<Vector3> points = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };

            List<int> faces = new List<int>(IcoFaces);
            for (int level = 0; level < subdivisions; level++)
            {
                List<int> refined = new List<int>();
                for (int f = 0; f < faces.Count; f += 3)
                {
                    int a = faces[f];
                    int b = faces[f + 1];
                    int c = faces[f + 2];
                    int ab = points.Count;
                    points.Add((points[a].normalized + points[b].normalized) * 0.5f);
                    int bc = points.Count;
                    points.Add((points[b].normalized + points[c].normalized) * 0.5f);
                    int ca = points.Count;
                    points.Add((points[c].normalized + points[a].normalized) * 0.5f);
                    refined.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }

                faces = refined;
            }

            Vector3 center = m.MultiplyPoint3x4(Vector3.zero);
            for (int f = 0; f < faces.Count; f += 3)
            {
                Triangle(
                    m.MultiplyPoint3x4(points[faces[f]].normalized * radius),
                    m.MultiplyPoint3x4(points[faces[f + 1]].normalized * radius),
                    m.MultiplyPoint3x4(points[faces[f + 2]].normalized * radius),
                    center);
            }

            return this;
        }

        // Box centred on the matrix origin; topScale narrows the top face (x, z) for wedges and trapezoids.
        public MeshBuilder Box(Matrix4x4 m, Vector3 size, Vector2 topScale)
        {
            piece++;
            Vector3 h = size * 0.5f;
            Vector3[] c =
            {
                new Vector3(-h.x, -h.y, -h.z), new Vector3(h.x, -h.y, -h.z), new Vector3(h.x, -h.y, h.z), new Vector3(-h.x, -h.y, h.z),
                new Vector3(-h.x * topScale.x, h.y, -h.z * topScale.y), new Vector3(h.x * topScale.x, h.y, -h.z * topScale.y),
                new Vector3(h.x * topScale.x, h.y, h.z * topScale.y), new Vector3(-h.x * topScale.x, h.y, h.z * topScale.y)
            };

            for (int i = 0; i < c.Length; i++)
            {
                c[i] = m.MultiplyPoint3x4(c[i]);
            }

            Vector3 inside = m.MultiplyPoint3x4(Vector3.zero);
            Quad(c[0], c[1], c[2], c[3], inside);
            Quad(c[4], c[5], c[6], c[7], inside);
            Quad(c[0], c[1], c[5], c[4], inside);
            Quad(c[1], c[2], c[6], c[5], inside);
            Quad(c[2], c[3], c[7], c[6], inside);
            Quad(c[3], c[0], c[4], c[7], inside);
            return this;
        }

        public MeshBuilder Box(Matrix4x4 m, Vector3 size)
        {
            return Box(m, size, Vector2.one);
        }

        // Box centred on the matrix origin whose top edges are cut (platforms, panels). bevel is the cut per axis.
        public MeshBuilder ChamferedBox(Matrix4x4 m, Vector3 size, Vector3 bevel)
        {
            piece++;
            Vector3 h = size * 0.5f;
            float mid = h.y - bevel.y;
            float topX = h.x - bevel.x;
            float topZ = h.z - bevel.z;
            Vector3[] c =
            {
                new Vector3(-h.x, -h.y, -h.z), new Vector3(h.x, -h.y, -h.z), new Vector3(h.x, -h.y, h.z), new Vector3(-h.x, -h.y, h.z),
                new Vector3(-h.x, mid, -h.z), new Vector3(h.x, mid, -h.z), new Vector3(h.x, mid, h.z), new Vector3(-h.x, mid, h.z),
                new Vector3(-topX, h.y, -topZ), new Vector3(topX, h.y, -topZ), new Vector3(topX, h.y, topZ), new Vector3(-topX, h.y, topZ)
            };

            for (int i = 0; i < c.Length; i++)
            {
                c[i] = m.MultiplyPoint3x4(c[i]);
            }

            Vector3 inside = m.MultiplyPoint3x4(Vector3.zero);
            Quad(c[0], c[1], c[2], c[3], inside);
            Quad(c[8], c[9], c[10], c[11], inside);
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                Quad(c[i], c[j], c[4 + j], c[4 + i], inside);
                Quad(c[4 + i], c[4 + j], c[8 + j], c[8 + i], inside);
            }

            return this;
        }

        // Torus around the local Y axis (halos, wheels, the ring of the eye of X.A.N.A.).
        public MeshBuilder Ring(Matrix4x4 m, float radius, float tube, int segments, int tubeSides = 4)
        {
            piece++;
            for (int s = 0; s < segments; s++)
            {
                float a0 = s * Mathf.PI * 2f / segments;
                float a1 = (s + 1) * Mathf.PI * 2f / segments;
                float mid = (a0 + a1) * 0.5f;
                Vector3 inside = m.MultiplyPoint3x4(new Vector3(Mathf.Cos(mid) * radius, 0f, Mathf.Sin(mid) * radius));
                for (int k = 0; k < tubeSides; k++)
                {
                    float p0 = (k + 0.5f) * Mathf.PI * 2f / tubeSides;
                    float p1 = (k + 1.5f) * Mathf.PI * 2f / tubeSides;
                    Quad(
                        m.MultiplyPoint3x4(TorusPoint(a0, p0, radius, tube)),
                        m.MultiplyPoint3x4(TorusPoint(a1, p0, radius, tube)),
                        m.MultiplyPoint3x4(TorusPoint(a1, p1, radius, tube)),
                        m.MultiplyPoint3x4(TorusPoint(a0, p1, radius, tube)),
                        inside);
                }
            }

            return this;
        }

        // Flat slice of a disc in the XZ plane, hinged at the origin and centred on +Z (Yumi's fans).
        public MeshBuilder Sector(Matrix4x4 m, float radius, float degrees, float thickness, int segments)
        {
            piece++;
            float half = thickness * 0.5f;
            Vector3 inside = m.MultiplyPoint3x4(new Vector3(0f, 0f, radius * 0.4f));
            Vector3 hingeTop = m.MultiplyPoint3x4(new Vector3(0f, half, 0f));
            Vector3 hingeBottom = m.MultiplyPoint3x4(new Vector3(0f, -half, 0f));
            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Deg2Rad * Mathf.Lerp(-degrees * 0.5f, degrees * 0.5f, i / (float)segments);
                float a1 = Mathf.Deg2Rad * Mathf.Lerp(-degrees * 0.5f, degrees * 0.5f, (i + 1) / (float)segments);
                Vector3 top0 = m.MultiplyPoint3x4(new Vector3(Mathf.Sin(a0) * radius, half, Mathf.Cos(a0) * radius));
                Vector3 top1 = m.MultiplyPoint3x4(new Vector3(Mathf.Sin(a1) * radius, half, Mathf.Cos(a1) * radius));
                Vector3 bottom0 = m.MultiplyPoint3x4(new Vector3(Mathf.Sin(a0) * radius, -half, Mathf.Cos(a0) * radius));
                Vector3 bottom1 = m.MultiplyPoint3x4(new Vector3(Mathf.Sin(a1) * radius, -half, Mathf.Cos(a1) * radius));
                Triangle(hingeTop, top0, top1, inside);
                Triangle(hingeBottom, bottom1, bottom0, inside);
                Quad(top0, top1, bottom1, bottom0, inside);
                if (i == 0)
                {
                    Quad(hingeTop, top0, bottom0, hingeBottom, inside);
                }

                if (i == segments - 1)
                {
                    Quad(hingeTop, top1, bottom1, hingeBottom, inside);
                }
            }

            return this;
        }

        public Mesh Build(string name)
        {
            int count = corners.Count;
            Vector3[] normals = new Vector3[count];
            Dictionary<long, Vector3> smooth = new Dictionary<long, Vector3>();
            long[] keys = new long[count];

            for (int t = 0; t < count; t += 3)
            {
                Vector3 normal = Vector3.Cross(corners[t + 1] - corners[t], corners[t + 2] - corners[t]).normalized;
                int owner = pieces[t / 3];
                for (int k = 0; k < 3; k++)
                {
                    normals[t + k] = normal;
                    long key = PositionKey(corners[t + k], owner);
                    keys[t + k] = key;
                    smooth.TryGetValue(key, out Vector3 sum);
                    smooth[key] = sum + normal;
                }
            }

            List<Vector3> smoothNormals = new List<Vector3>(count);
            int[] indices = new int[count];
            for (int i = 0; i < count; i++)
            {
                smoothNormals.Add(smooth[keys[i]].normalized);
                indices[i] = i;
            }

            Mesh mesh = new Mesh { name = name };
            if (count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(corners);
            mesh.SetNormals(normals);
            mesh.SetUVs(3, smoothNormals);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector3 TorusPoint(float around, float tubeAngle, float radius, float tube)
        {
            float ring = radius + Mathf.Cos(tubeAngle) * tube;
            return new Vector3(Mathf.Cos(around) * ring, Mathf.Sin(tubeAngle) * tube, Mathf.Sin(around) * ring);
        }

        private static long PositionKey(Vector3 p, int owner)
        {
            long x = Mathf.RoundToInt(p.x * 1000f) & 0xFFFFF;
            long y = Mathf.RoundToInt(p.y * 1000f) & 0xFFFFF;
            long z = Mathf.RoundToInt(p.z * 1000f) & 0xFFFFF;
            return ((long)owner << 60) ^ (x << 40) ^ (y << 20) ^ z;
        }
    }
}
