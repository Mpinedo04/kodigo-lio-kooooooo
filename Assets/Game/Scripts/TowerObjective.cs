using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class TowerObjective : MonoBehaviour
    {
        [SerializeField] private AelitaCompanion aelita;
        [SerializeField] private float requiredAelitaDistance = 5f;
        [SerializeField] private float activationSeconds = 4f;
        [SerializeField] private Renderer towerRenderer;
        [SerializeField] private Color activeColor = new Color(1f, 0.1f, 0.1f, 1f);
        [SerializeField] private Color clearedColor = new Color(0.2f, 0.9f, 1f, 1f);

        private float progress;

        public bool IsCleared { get; private set; }
        public float Progress01 => Mathf.Clamp01(progress / activationSeconds);
        public float RequiredAelitaDistance => requiredAelitaDistance;

        public void SetAelita(AelitaCompanion companion)
        {
            aelita = companion;
        }

        private void Start()
        {
            ApplyColor(activeColor);
        }

        private void Update()
        {
            if (IsCleared || aelita == null || !aelita.IsNearTower(this))
            {
                return;
            }

            if (Input.GetKey(KeyCode.E))
            {
                progress += Time.deltaTime;
                if (progress >= activationSeconds)
                {
                    IsCleared = true;
                    ApplyColor(clearedColor);
                }
            }
        }

        private void ApplyColor(Color color)
        {
            if (towerRenderer != null && towerRenderer.sharedMaterial != null)
            {
                towerRenderer.material.color = color;
                towerRenderer.material.SetColor("_EmissionColor", color * 1.6f);
            }
        }
    }
}
