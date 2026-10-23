using UnityEngine;

namespace CodeLyokoFanGame
{
    // Look of a Lyoko tower (F7.6): the halo takes the colour of whoever controls it.
    public sealed class TowerVisual : MonoBehaviour
    {
        [SerializeField] private Renderer halo;
        [SerializeField] private Renderer column;
        [Tooltip("Light stripes on the column; they share the halo colour.")]
        [SerializeField] private Renderer[] accents = new Renderer[0];
        [SerializeField] private TowerState initialState = TowerState.Neutral;
        [SerializeField] private float pulseSpeed = 2f;

        private Material tinted;
        private Color current;
        private bool started;

        public TowerState State { get; private set; }

        public static Color ColorOf(TowerState state)
        {
            switch (state)
            {
                case TowerState.Xana:
                    return new Color(1f, 0.08f, 0.06f);
                case TowerState.Jeremie:
                    return new Color(0.15f, 1f, 0.35f);
                case TowerState.FranzHopper:
                    return Color.white;
                default:
                    return new Color(0.15f, 0.5f, 1f);
            }
        }

        public void Configure(Renderer haloRenderer, Renderer columnRenderer, TowerState state, Renderer[] accentRenderers = null)
        {
            halo = haloRenderer;
            column = columnRenderer;
            initialState = state;
            accents = accentRenderers ?? new Renderer[0];
        }

        private void Awake()
        {
            SetState(initialState);
        }

        public void SetState(TowerState state)
        {
            bool changed = started && state != State;
            State = state;
            if (halo == null || !Application.isPlaying)
            {
                return;
            }

            if (tinted == null)
            {
                tinted = halo.material;
                for (int i = 0; i < accents.Length; i++)
                {
                    if (accents[i] != null)
                    {
                        accents[i].sharedMaterial = tinted;
                    }
                }
            }

            current = ColorOf(state);
            tinted.SetColor("_BaseColor", current);
            started = true;

            // Deactivation flash (F9.25).
            if (changed)
            {
                ShockwaveEffect.Spawn(halo.transform.position, 7f, current);
                ShockwaveEffect.Spawn(transform.position + Vector3.up * 0.6f, 5f, current);
            }
        }

        private void Update()
        {
            if (tinted != null)
            {
                float pulse = 1.6f + Mathf.Sin(Time.time * pulseSpeed) * 0.6f;
                tinted.SetColor("_EmissionColor", current * pulse);
            }
        }
    }
}
