using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame
{
    // Terrain raised by an ability for a while (Yumi's wall, Aelita's bridges and walls).
    // It sits on the Environment layer, so it stops lasers and can be walked on.
    public sealed class TemporaryStructure : MonoBehaviour
    {
        public enum Appear
        {
            Rise,
            Grow
        }

        private const float AppearTime = 0.3f;

        private Vector3 finalPosition;
        private Vector3 finalScale;
        private Appear appear;
        private float lifetime;
        private float age;

        public static GameObject Create(string name, Vector3 center, Quaternion rotation, Vector3 size, Color color, float lifetime, Appear appear, bool blocksMonsters)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetPositionAndRotation(center, rotation);
            block.transform.localScale = size;
            block.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Box(Vector3.one, new Vector2(0.94f, 0.94f));
            LyokoMaterials.Paint(block.GetComponent<Renderer>(), color, 0.15f, 1f);
            GameLayers.SetRecursively(block, GameLayers.Environment);

            if (blocksMonsters)
            {
                NavMeshObstacle obstacle = block.AddComponent<NavMeshObstacle>();
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.size = Vector3.one;
                obstacle.carving = true;
            }

            TemporaryStructure structure = block.AddComponent<TemporaryStructure>();
            structure.finalPosition = center;
            structure.finalScale = size;
            structure.appear = appear;
            structure.lifetime = Mathf.Max(AppearTime * 2f, lifetime);
            structure.Apply(0f);
            return block;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float shown = Mathf.Min(Mathf.Clamp01(age / AppearTime), Mathf.Clamp01((lifetime - age) / AppearTime));
            Apply(shown);
            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void Apply(float shown)
        {
            if (appear == Appear.Rise)
            {
                transform.position = finalPosition - Vector3.up * finalScale.y * (1f - shown);
            }
            else
            {
                transform.localScale = new Vector3(finalScale.x, finalScale.y, finalScale.z * Mathf.Max(0.02f, shown));
            }
        }
    }
}
