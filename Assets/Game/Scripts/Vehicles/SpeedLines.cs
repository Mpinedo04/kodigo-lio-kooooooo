using UnityEngine;

namespace CodeLyokoFanGame
{
    // Placeholder speed effect: thin streaks rush past the camera when a vehicle goes fast.
    // CameraRig attaches it to the main camera and feeds the intensity (0..1).
    public sealed class SpeedLines : MonoBehaviour
    {
        private const int IgnoreRaycastLayer = 2;

        [SerializeField] private int count = 26;
        [Tooltip("Distance from the view axis: streaks stay near the edges of the screen, away from the rider.")]
        [SerializeField] private float minRadius = 2.4f;
        [SerializeField] private float maxRadius = 4.6f;
        [SerializeField] private float depth = 14f;
        [SerializeField] private float streakSpeed = 60f;
        [SerializeField] private float streakLength = 2.6f;
        [SerializeField] private float thickness = 0.025f;
        [SerializeField] private Color color = new Color(0.75f, 0.97f, 1f);

        private Transform[] streaks;
        private float intensity;
        private bool visible;

        public static SpeedLines Attach(Transform cameraTransform)
        {
            GameObject host = new GameObject("SpeedLines");
            host.layer = IgnoreRaycastLayer;
            host.transform.SetParent(cameraTransform, false);
            return host.AddComponent<SpeedLines>();
        }

        public void SetIntensity(float value)
        {
            intensity = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            streaks = new Transform[count];
            Material shared = null;
            for (int i = 0; i < count; i++)
            {
                GameObject streak = GameObject.CreatePrimitive(PrimitiveType.Cube);
                streak.name = "Streak_" + i;
                streak.GetComponent<MeshFilter>().sharedMesh = LowPolyMesh.Box(Vector3.one);
                streak.layer = IgnoreRaycastLayer;
                DestroyImmediate(streak.GetComponent<Collider>());
                streak.transform.SetParent(transform, false);
                Renderer renderer = streak.GetComponent<Renderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                if (shared == null)
                {
                    shared = renderer.material;
                    shared.color = color;
                    if (shared.HasProperty("_EmissionColor"))
                    {
                        shared.EnableKeyword("_EMISSION");
                        shared.SetColor("_EmissionColor", color * 1.5f);
                    }
                }
                else
                {
                    renderer.sharedMaterial = shared;
                }

                streaks[i] = streak.transform;
                Respawn(streaks[i], Random.Range(0f, depth));
                streak.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            bool show = intensity > 0.02f;
            if (show != visible)
            {
                visible = show;
                for (int i = 0; i < streaks.Length; i++)
                {
                    streaks[i].gameObject.SetActive(show);
                }
            }

            if (!show)
            {
                return;
            }

            float dt = Time.deltaTime;
            Vector3 scale = new Vector3(thickness, thickness, streakLength * intensity);
            for (int i = 0; i < streaks.Length; i++)
            {
                Transform streak = streaks[i];
                Vector3 local = streak.localPosition;
                local.z -= streakSpeed * (0.5f + intensity) * dt;
                streak.localPosition = local;
                streak.localScale = scale;
                if (local.z < -1f)
                {
                    Respawn(streak, depth);
                }
            }
        }

        private void Respawn(Transform streak, float z)
        {
            Vector2 ring = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
            streak.localPosition = new Vector3(ring.x, ring.y, z);
            streak.localRotation = Quaternion.identity;
        }
    }
}
