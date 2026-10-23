using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame.EditorTools
{
    // Scene pieces for the F05 abilities: rocks for Yumi's telekinesis and Creativity zones for Aelita.
    public static class AbilityPropsBuilder
    {
        public static TelekinesisTarget CreateRock(string name, Vector3 position, float size, Material material)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = name;
            rock.transform.position = position;
            rock.transform.localScale = Vector3.one * size;
            rock.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            rock.GetComponent<Renderer>().sharedMaterial = material;

            Rigidbody body = rock.AddComponent<Rigidbody>();
            body.mass = 40f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            NavMeshObstacle obstacle = rock.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            TelekinesisTarget target = rock.AddComponent<TelekinesisTarget>();
            GameLayers.SetRecursively(rock, GameLayers.Environment);
            return target;
        }

        public static CreativityAnchor CreateBridgeZone(string name, Vector3 start, Vector3 end, Material marker)
        {
            CreativityAnchor anchor = CreateAnchor(name, start, Quaternion.LookRotation(Vector3.ProjectOnPlane(end - start, Vector3.up)), marker);
            Transform endPoint = new GameObject("BridgeEnd").transform;
            endPoint.SetParent(anchor.transform);
            endPoint.position = end;
            anchor.Configure(CreativityKind.Bridge, endPoint);
            return anchor;
        }

        public static CreativityAnchor CreateWallZone(string name, Vector3 position, Quaternion facing, Material marker)
        {
            CreativityAnchor anchor = CreateAnchor(name, position, facing, marker);
            anchor.Configure(CreativityKind.Wall, null);
            return anchor;
        }

        private static CreativityAnchor CreateAnchor(string name, Vector3 position, Quaternion rotation, Material marker)
        {
            GameObject root = new GameObject(name);
            root.transform.SetPositionAndRotation(position, rotation);

            // Glowing disc so the player can see where Creativity works.
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "CreativityMarker";
            disc.transform.SetParent(root.transform, false);
            disc.transform.localPosition = Vector3.up * 0.03f;
            disc.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);
            disc.GetComponent<Renderer>().sharedMaterial = marker;
            Object.DestroyImmediate(disc.GetComponent<Collider>());

            return root.AddComponent<CreativityAnchor>();
        }
    }
}
