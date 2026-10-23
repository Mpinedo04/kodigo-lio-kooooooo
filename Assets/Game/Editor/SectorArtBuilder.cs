using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // F09 9D: modular kit to dress sectors (F9.20): Digital Sea, cliffs under the platforms, giant trees
    // rising from the sea and floating data. Colours come from the SectorDefinition look, so the same pieces
    // dress the Desert, Ice or Mountain later.
    public static class SectorArtBuilder
    {
        private const string ArtFolder = "Assets/Game/Generated/Art";

        public static GameObject CreateDigitalSea(Vector3 center, float size)
        {
            GameObject sea = new GameObject("DigitalSea");
            sea.transform.position = center;
            sea.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Grid(size, 140);
            sea.AddComponent<MeshRenderer>().sharedMaterial = SeaMaterial();
            return sea;
        }

        public static Material SeaMaterial()
        {
            Directory.CreateDirectory(ArtFolder);
            string path = ArtFolder + "/DigitalSea.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("CodeLyoko/DigitalSea"));
                AssetDatabase.CreateAsset(material, path);
            }

            return material;
        }

        public static SectorDefinition Sector(SectorId id)
        {
            SectorDefinition sector = LookBuilder.LoadSector(id);
            return sector != null ? sector : ScriptableObject.CreateInstance<SectorDefinition>();
        }

        // A rocky column under a platform that narrows as it sinks towards the sea (visual only).
        public static GameObject AddCliff(GameObject platform, Color terrain, float depth)
        {
            Vector3 size = platform.transform.localScale;
            Vector3 bottomOfPlatform = platform.transform.position - Vector3.up * size.y * 0.5f;
            GameObject cliff = new GameObject(platform.name + "_Cliff");
            cliff.transform.position = bottomOfPlatform - Vector3.up * depth * 0.5f;
            cliff.transform.localScale = new Vector3(size.x * 0.69f, depth, size.z * 0.69f);
            cliff.AddComponent<MeshFilter>().sharedMesh = LowPolyMesh.Box(Vector3.one, new Vector2(1.45f, 1.45f));
            cliff.AddComponent<MeshRenderer>().sharedMaterial = LyokoMaterials.Toon(terrain);
            return cliff;
        }

        // Giant tree of the Forest sector: trunk from the Digital Sea, root flare, a branch and leaf clusters.
        public static GameObject CreateTree(string name, Vector3 basePosition, float height, float radius, Color trunk, Color leaves)
        {
            GameObject root = new GameObject(name);
            root.transform.position = basePosition;

            Piece(root.transform, "Trunk", LowPolyMesh.Prism(8, radius, radius * 0.7f, height), Vector3.zero, Quaternion.identity, Vector3.one, trunk, 1f);
            Piece(root.transform, "RootFlare", LowPolyMesh.Prism(8, radius * 1.7f, radius, height * 0.08f), Vector3.zero, Quaternion.identity, Vector3.one, trunk, 1f);
            Piece(root.transform, "Branch", LowPolyMesh.Prism(6, radius * 0.35f, radius * 0.15f, radius * 3.2f), new Vector3(0f, height * 0.72f, 0f), Quaternion.Euler(0f, 0f, -58f), Vector3.one, trunk, 1f);
            Piece(root.transform, "Canopy", LowPolyMesh.Gem(radius * 2.6f, 1), new Vector3(0f, height, 0f), Quaternion.identity, new Vector3(1f, 0.62f, 1f), leaves, 1f);
            Piece(root.transform, "CanopySide", LowPolyMesh.Gem(radius * 1.7f, 1), new Vector3(radius * 1.8f, height * 0.9f, radius * 0.6f), Quaternion.identity, new Vector3(1f, 0.6f, 1f), leaves, 1f);
            Piece(root.transform, "BranchLeaves", LowPolyMesh.Gem(radius * 1.2f, 1), new Vector3(radius * 2.7f, height * 0.72f + radius * 1.7f, 0f), Quaternion.identity, new Vector3(1f, 0.65f, 1f), leaves, 1f);

            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.radius = radius;
            collider.height = height;
            collider.center = Vector3.up * height * 0.5f;
            GameLayers.SetRecursively(root, GameLayers.Environment);
            return root;
        }

        // A few glowing data cubes that float and turn (visual only).
        public static GameObject CreateDataCluster(string name, Vector3 center, int count, float spread, Color accent)
        {
            GameObject root = new GameObject(name);
            root.transform.position = center;
            Random.State saved = Random.state;
            Random.InitState(name.GetHashCode());
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-spread, spread), Random.Range(-spread * 0.4f, spread * 0.4f), Random.Range(-spread, spread));
                float size = Random.Range(0.12f, 0.32f);
                GameObject bit = Piece(root.transform, "DataBit_" + i, LowPolyMesh.Box(Vector3.one), offset, Random.rotation, Vector3.one * size, accent, 0f);
                bit.GetComponent<MeshRenderer>().sharedMaterial = LyokoMaterials.Toon(accent, 1.8f);
                bit.AddComponent<Spin>().Configure(Random.onUnitSphere, Random.Range(25f, 70f), Random.Range(0.1f, 0.35f), Random.Range(0.6f, 1.4f));
            }

            Random.state = saved;
            return root;
        }

        // Forest sector dressing around the vertical slice paths (x within +-15, z -22..57).
        public static void DressForest(SectorDefinition look)
        {
            Color trunk = look.terrainColor;
            Color leaves = Color.Lerp(look.groundColor, new Color(0.05f, 0.35f, 0.15f), 0.35f);
            GameObject forest = new GameObject("ForestDressing");
            int index = 0;
            for (int row = 0; row < 10; row++)
            {
                float z = -26f + row * 9.5f;
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * (21f + (row * 7 + side * 3) % 11);
                    float height = 30f + (row * 5 + side) % 12;
                    float radius = 1.6f + ((row + side + 3) % 4) * 0.35f;
                    CreateTree("GiantTree_" + index++, new Vector3(x, -10f, z), height, radius, trunk, leaves).transform.SetParent(forest.transform, true);
                }
            }

            Vector3[] clusters =
            {
                new Vector3(-7f, 2.2f, 2f), new Vector3(7.5f, 2.8f, 12f), new Vector3(-17f, 3f, 28f),
                new Vector3(17f, 2.5f, 36f), new Vector3(-12f, 4f, 52f), new Vector3(12f, 3.5f, -18f)
            };

            for (int i = 0; i < clusters.Length; i++)
            {
                CreateDataCluster("DataCluster_" + i, clusters[i], 7, 1.6f, look.accentColor).transform.SetParent(forest.transform, true);
            }
        }

        private static GameObject Piece(Transform parent, string name, Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Color color, float outline)
        {
            GameObject piece = new GameObject(name);
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = position;
            piece.transform.localRotation = rotation;
            piece.transform.localScale = scale;
            piece.AddComponent<MeshFilter>().sharedMesh = mesh;
            piece.AddComponent<MeshRenderer>().sharedMaterial = LyokoMaterials.Toon(color, 0f, outline);
            return piece;
        }
    }
}
