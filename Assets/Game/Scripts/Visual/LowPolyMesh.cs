using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Shared low-poly meshes, built once per shape and cached. When the generators run in the Editor,
    // Persist (set by ArtAssetPersistence) saves them as assets so the scenes keep them.
    public static class LowPolyMesh
    {
        private static readonly Dictionary<string, Mesh> Cache = new Dictionary<string, Mesh>();

        public static Func<string, Mesh, Mesh> Persist { get; set; }

        public static Mesh Get(string key, Action<MeshBuilder> build)
        {
            if (Cache.TryGetValue(key, out Mesh cached) && cached != null)
            {
                return cached;
            }

            MeshBuilder builder = new MeshBuilder();
            build(builder);
            Mesh mesh = builder.Build(key);
            if (!Application.isPlaying && Persist != null)
            {
                mesh = Persist(key, mesh);
            }

            Cache[key] = mesh;
            return mesh;
        }

        public static Mesh Prism(int sides, float bottomRadius, float topRadius, float height, float depth = 1f)
        {
            string key = "Prism_" + sides + "_" + F(bottomRadius) + "_" + F(topRadius) + "_" + F(height) + "_" + F(depth);
            return Get(key, b => b.Prism(Matrix4x4.identity, sides, bottomRadius, topRadius, height, depth));
        }

        public static Mesh Gem(float radius, int subdivisions = 1)
        {
            return Get("Gem_" + F(radius) + "_" + subdivisions, b => b.Gem(Matrix4x4.identity, radius, subdivisions));
        }

        public static Mesh Box(Vector3 size, Vector2 topScale)
        {
            string key = "Box_" + F(size.x) + "_" + F(size.y) + "_" + F(size.z) + "_" + F(topScale.x) + "_" + F(topScale.y);
            return Get(key, b => b.Box(Matrix4x4.identity, size, topScale));
        }

        public static Mesh Box(Vector3 size)
        {
            return Box(size, Vector2.one);
        }

        // Faceted stand-ins the same size as Unity's primitives, so nothing keeps the smooth placeholder look.
        // Planes and quads have none (null).
        public static Mesh Faceted(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Cube:
                    return Box(Vector3.one, new Vector2(0.9f, 0.9f));
                case PrimitiveType.Sphere:
                    return Gem(0.5f, 1);
                case PrimitiveType.Cylinder:
                    return Get("UnitCylinder12", b => b.Prism(Matrix4x4.Translate(new Vector3(0f, -1f, 0f)), 12, 0.5f, 0.5f, 2f));
                case PrimitiveType.Capsule:
                    return Get("UnitCapsule", b => b.Gem(Matrix4x4.Scale(new Vector3(1f, 2f, 1f)), 0.5f, 1));
                default:
                    return null;
            }
        }

        // Swaps the mesh of a primitive for its faceted stand-in.
        public static void Facet(GameObject primitive, PrimitiveType type)
        {
            Mesh mesh = Faceted(type);
            MeshFilter filter = primitive != null ? primitive.GetComponent<MeshFilter>() : null;
            if (mesh != null && filter != null)
            {
                filter.sharedMesh = mesh;
            }
        }

        // Unit block with chamfered top edges for an object scaled to size: the cut stays the same in metres
        // whatever the size (platforms, panels).
        public static Mesh Slab(Vector3 size, float bevel = 0.3f)
        {
            size = Vector3.Max(size, Vector3.one * 0.01f);
            float cut = Mathf.Min(bevel, 0.25f * Mathf.Min(size.x, Mathf.Min(size.y, size.z)));
            Vector3 unitCut = new Vector3(cut / size.x, cut / size.y, cut / size.z);
            string key = "Slab_" + F(size.x) + "_" + F(size.y) + "_" + F(size.z) + "_" + F(cut);
            return Get(key, b => b.ChamferedBox(Matrix4x4.identity, Vector3.one, unitCut));
        }

        public static Mesh Ring(float radius, float tube, int segments, int tubeSides = 4)
        {
            string key = "Ring_" + F(radius) + "_" + F(tube) + "_" + segments + "_" + tubeSides;
            return Get(key, b => b.Ring(Matrix4x4.identity, radius, tube, segments, tubeSides));
        }

        public static Mesh Sector(float radius, float degrees, float thickness, int segments)
        {
            string key = "Sector_" + F(radius) + "_" + F(degrees) + "_" + F(thickness) + "_" + segments;
            return Get(key, b => b.Sector(Matrix4x4.identity, radius, degrees, thickness, segments));
        }

        // Subdivided flat grid in XZ (the Digital Sea needs vertices for its waves).
        public static Mesh Grid(float size, int cells)
        {
            string key = "Grid_" + F(size) + "_" + cells;
            if (Cache.TryGetValue(key, out Mesh cached) && cached != null)
            {
                return cached;
            }

            int line = cells + 1;
            Vector3[] vertices = new Vector3[line * line];
            Vector3[] normals = new Vector3[vertices.Length];
            int[] triangles = new int[cells * cells * 6];
            for (int z = 0; z < line; z++)
            {
                for (int x = 0; x < line; x++)
                {
                    vertices[z * line + x] = new Vector3((x / (float)cells - 0.5f) * size, 0f, (z / (float)cells - 0.5f) * size);
                    normals[z * line + x] = Vector3.up;
                }
            }

            int t = 0;
            for (int z = 0; z < cells; z++)
            {
                for (int x = 0; x < cells; x++)
                {
                    int i = z * line + x;
                    triangles[t++] = i;
                    triangles[t++] = i + line;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + 1;
                    triangles[t++] = i + line;
                    triangles[t++] = i + line + 1;
                }
            }

            Mesh mesh = new Mesh { name = key };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            if (!Application.isPlaying && Persist != null)
            {
                mesh = Persist(key, mesh);
            }

            Cache[key] = mesh;
            return mesh;
        }

        // The eye of X.A.N.A. (F9.15): a ring with a pupil, one line above and three below, facing +Z.
        public static Mesh XanaEye()
        {
            return Get("XanaEye", b =>
            {
                Matrix4x4 faceForward = Matrix4x4.Rotate(Quaternion.Euler(90f, 0f, 0f));
                b.Gem(Matrix4x4.Translate(new Vector3(0f, 0f, -0.08f)), 0.2f, 1);
                b.Ring(faceForward, 0.3f, 0.06f, 16, 4);
                b.Prism(faceForward * Matrix4x4.Translate(new Vector3(0f, -0.05f, 0f)), 10, 0.12f, 0.12f, 0.1f);
                b.Box(Matrix4x4.Translate(new Vector3(0f, 0.45f, 0f)), new Vector3(0.07f, 0.22f, 0.08f));
                b.Box(Matrix4x4.Translate(new Vector3(0f, -0.48f, 0f)), new Vector3(0.07f, 0.28f, 0.08f));
                b.Box(Matrix4x4.TRS(new Vector3(-0.17f, -0.43f, 0f), Quaternion.Euler(0f, 0f, -20f), Vector3.one), new Vector3(0.06f, 0.2f, 0.08f));
                b.Box(Matrix4x4.TRS(new Vector3(0.17f, -0.43f, 0f), Quaternion.Euler(0f, 0f, 20f), Vector3.one), new Vector3(0.06f, 0.2f, 0.08f));
            });
        }

        private static string F(float value)
        {
            return Mathf.RoundToInt(value * 1000f).ToString();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCache()
        {
            Cache.Clear();
        }
    }
}
