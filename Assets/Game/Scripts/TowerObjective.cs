using UnityEngine;

namespace CodeLyokoFanGame
{
    // The tower X.A.N.A. activated. With Aelita next to it, hold Interact to send her in (TowerInterior);
    // without an interior, holding Interact deactivates it directly.
    public sealed class TowerObjective : MonoBehaviour
    {
        [SerializeField] private AelitaCompanion aelita;
        [SerializeField] private InputReader input;
        [SerializeField] private TowerVisual visual;
        [SerializeField] private TowerInterior interior;
        [SerializeField] private float requiredAelitaDistance = 6f;
        [Tooltip("Without an interior: seconds holding Interact to deactivate the tower.")]
        [SerializeField] private float activationSeconds = 4f;
        [Tooltip("With an interior: seconds holding Interact before Aelita goes in.")]
        [SerializeField] private float enterHoldSeconds = 0.6f;

        private float progress;
        private float enterProgress;

        public bool IsCleared { get; private set; }
        public float Progress01 => interior != null ? interior.Progress01 : Mathf.Clamp01(progress / activationSeconds);
        public float RequiredAelitaDistance => requiredAelitaDistance;
        public TowerState State => visual != null ? visual.State : (IsCleared ? TowerState.Neutral : TowerState.Xana);

        public void SetAelita(AelitaCompanion companion)
        {
            aelita = companion;
        }

        public void Configure(TowerVisual towerVisual, TowerInterior towerInterior)
        {
            visual = towerVisual;
            interior = towerInterior;
        }

        private void Start()
        {
            if (visual != null)
            {
                visual.SetState(TowerState.Xana);
            }

            GameManager.RaiseEvent(GameEventId.TowerActivated);
        }

        private void Update()
        {
            if (IsCleared || (interior != null && interior.IsRunning) || aelita == null || !aelita.isActiveAndEnabled || !aelita.IsNearTower(this))
            {
                enterProgress = 0f;
                return;
            }

            if (input == null || !input.InteractHeld)
            {
                enterProgress = 0f;
                return;
            }

            if (interior != null)
            {
                enterProgress += Time.deltaTime;
                if (enterProgress >= enterHoldSeconds)
                {
                    interior.Begin(this, aelita);
                }

                return;
            }

            progress += Time.deltaTime;
            if (progress >= activationSeconds)
            {
                Deactivate();
            }
        }

        // CODE: LYOKO entered: the tower goes back to neutral.
        public void Deactivate()
        {
            if (IsCleared)
            {
                return;
            }

            IsCleared = true;
            if (visual != null)
            {
                visual.SetState(TowerState.Neutral);
            }

            GameManager.RaiseEvent(GameEventId.TowerDeactivated);
        }
    }
}
