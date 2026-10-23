using Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // F07: the tower prefab (halo coloured by state), the tower interior where Aelita types CODE: LYOKO,
    // and pairs of passage towers.
    public static class TowerBuilder
    {
        private const string TowerPrefabPath = "Assets/Game/Generated/Prefabs/LyokoTower.prefab";

        public static GameObject BuildTowerPrefab()
        {
            Material white = CodeLyokoSceneBuilder.Material("Lyoko_Tower_White", new Color(0.82f, 0.86f, 0.9f), 0.05f);
            Material halo = CodeLyokoSceneBuilder.Material("Lyoko_Tower_Halo", TowerVisual.ColorOf(TowerState.Neutral), 1.6f);

            GameObject root = new GameObject("LyokoTower");
            GameObject column = Shape(PrimitiveType.Cylinder, "TowerColumn", root.transform, new Vector3(0f, 3f, 0f), new Vector3(2.8f, 3f, 2.8f), white, true);
            column.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Get("UnitCylinder10", b => b.Prism(Matrix4x4.Translate(new Vector3(0f, -1f, 0f)), 10, 0.5f, 0.5f, 2f));
            GameLayers.SetRecursively(column, GameLayers.Environment);
            GameObject top = Shape(PrimitiveType.Cylinder, "TowerTop", root.transform, new Vector3(0f, 6.1f, 0f), new Vector3(2.4f, 0.8f, 2.4f), white, false);
            top.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Gem(0.5f, 1);

            // F09: the halo is a ring that turns and pulses; four light stripes run up the column in the same colour.
            GameObject ring = Shape(PrimitiveType.Cylinder, "TowerHalo", root.transform, new Vector3(0f, 3.4f, 0f), Vector3.one * 3.8f, halo, false);
            ring.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.03f, 28, 4);
            ring.AddComponent<Spin>().Configure(Vector3.up, 25f, 0.25f, 1.2f);
            Renderer[] stripes = new Renderer[4];
            for (int i = 0; i < stripes.Length; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                GameObject stripe = Shape(PrimitiveType.Cube, "TowerStripe_" + i, root.transform, new Vector3(Mathf.Cos(angle) * 1.4f, 3f, Mathf.Sin(angle) * 1.4f), new Vector3(0.09f, 5.4f, 0.09f), halo, false);
                stripes[i] = stripe.GetComponent<Renderer>();
            }

            root.AddComponent<TowerVisual>().Configure(ring.GetComponent<Renderer>(), column.GetComponent<Renderer>(), TowerState.Neutral, stripes);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, TowerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static TowerVisual PlaceTower(GameObject prefab, string name, Vector3 position, Quaternion rotation, TowerState state)
        {
            GameObject tower = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tower.name = name;
            tower.transform.SetPositionAndRotation(position, rotation);
            TowerVisual visual = tower.GetComponent<TowerVisual>();
            Renderer[] stripes = new Renderer[4];
            for (int i = 0; i < stripes.Length; i++)
            {
                stripes[i] = tower.transform.Find("TowerStripe_" + i).GetComponent<Renderer>();
            }

            visual.Configure(tower.transform.Find("TowerHalo").GetComponent<Renderer>(), tower.transform.Find("TowerColumn").GetComponent<Renderer>(), state, stripes);
            return visual;
        }

        public static TowerObjective CreateActivatedTower(GameObject prefab, Vector3 position, AelitaCompanion aelita, InputReader input, TowerInterior interior)
        {
            TowerVisual visual = PlaceTower(prefab, "ActivatedTower", position, Quaternion.identity, TowerState.Xana);
            TowerObjective objective = visual.gameObject.AddComponent<TowerObjective>();
            objective.SetAelita(aelita);
            objective.Configure(visual, interior);
            CodeLyokoSceneBuilder.SetPrivate(objective, "input", input);
            return objective;
        }

        // A separate little room far from the level: floor, upper platform and the panel. Its own camera takes over.
        public static TowerInterior CreateInterior(Vector3 origin, CharacterSwitcher switcher, InputReader input)
        {
            Material white = CodeLyokoSceneBuilder.Material("Lyoko_Tower_White", new Color(0.82f, 0.86f, 0.9f), 0.05f);
            Material dark = CodeLyokoSceneBuilder.Material("Lyoko_Dark_Metal", new Color(0.06f, 0.08f, 0.09f), 0.1f);
            Material cyan = CodeLyokoSceneBuilder.Material("Lyoko_Cyan_Emission", new Color(0.1f, 0.85f, 1f), 1.7f);

            GameObject root = new GameObject("TowerInterior");
            root.transform.position = origin;
            GameObject floor = Shape(PrimitiveType.Cylinder, "InteriorFloor", root.transform, new Vector3(0f, -0.1f, 0f), new Vector3(12f, 0.1f, 12f), dark, true);
            GameObject upper = Shape(PrimitiveType.Cylinder, "UpperPlatform", root.transform, new Vector3(0f, 9.9f, 0f), new Vector3(5f, 0.1f, 5f), white, true);
            GameLayers.SetRecursively(floor, GameLayers.Environment);
            GameLayers.SetRecursively(upper, GameLayers.Environment);
            GameObject panel = Shape(PrimitiveType.Cube, "CodePanel", root.transform, new Vector3(0f, 11.3f, 2.2f), new Vector3(2.4f, 1.4f, 0.1f), cyan, false);
            panel.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Slab(panel.transform.localScale, 0.03f);
            // F09: two circles of light on the floor, like the tower interiors of the series.
            for (int i = 0; i < 2; i++)
            {
                GameObject floorRing = Shape(PrimitiveType.Cylinder, "FloorRing_" + i, root.transform, new Vector3(0f, 0.02f, 0f), Vector3.one * (6f + i * 3.5f), cyan, false);
                floorRing.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.006f, 32, 4);
            }

            for (int i = 1; i <= 3; i++)
            {
                GameObject dataRing = Shape(PrimitiveType.Cylinder, "DataRing_" + i, root.transform, new Vector3(0f, i * 3f, 0f), Vector3.one * 11f, cyan, false);
                dataRing.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Ring(0.5f, 0.008f, 32, 4);
                dataRing.AddComponent<Spin>().Configure(Vector3.up, i % 2 == 0 ? -20f : 20f, 0.15f, 0.8f);
            }

            Transform bottom = Point("Bottom", root.transform, Vector3.zero);
            Transform top = Point("Top", root.transform, Vector3.up * 10f);
            Transform avatar = Point("AelitaInTower", root.transform, Vector3.zero);
            WarriorModel.Build(LyokoCharacter.Aelita, avatar);

            GameObject cameraObject = new GameObject("CM_TowerInterior");
            cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(7f, 7f, -7f);
            CinemachineVirtualCamera interiorCamera = cameraObject.AddComponent<CinemachineVirtualCamera>();
            interiorCamera.m_LookAt = avatar;
            interiorCamera.m_Lens.FieldOfView = 55f;
            interiorCamera.Priority = 0;
            interiorCamera.AddCinemachineComponent<CinemachineComposer>();

            TowerInterior interior = root.AddComponent<TowerInterior>();
            interior.Configure(bottom, top, avatar, interiorCamera, switcher, input);
            return interior;
        }

        // Two neutral towers linked to each other: walking into one brings the team out of the other.
        public static void CreatePassagePair(GameObject prefab, string nameA, Vector3 positionA, string nameB, Vector3 positionB)
        {
            PassageTower a = PlaceTower(prefab, nameA, positionA, Quaternion.identity, TowerState.Neutral).gameObject.AddComponent<PassageTower>();
            PassageTower b = PlaceTower(prefab, nameB, positionB, Quaternion.identity, TowerState.Neutral).gameObject.AddComponent<PassageTower>();
            a.Link(b);
            b.Link(a);
        }

        private static GameObject Shape(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, bool keepCollider)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            LowPolyMesh.Facet(shape, type);
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = localPosition;
            shape.transform.localScale = localScale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider)
            {
                Object.DestroyImmediate(shape.GetComponent<Collider>());
            }

            return shape;
        }

        private static Transform Point(string name, Transform parent, Vector3 localPosition)
        {
            Transform point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.localPosition = localPosition;
            return point;
        }
    }
}
