using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class VehicleRig : MonoBehaviour
    {
        [SerializeField] private VehicleStyle style;
        [SerializeField] private float speedMultiplier = 1.55f;
        [SerializeField] private bool canFly = true;
        [SerializeField] private float hoverBob = 0.08f;
        [SerializeField] private float hoverRate = 5f;

        private Vector3 localStart;

        public float SpeedMultiplier => speedMultiplier;
        public bool CanFly => canFly;

        public void Configure(VehicleStyle newStyle)
        {
            style = newStyle;

            switch (style)
            {
                case VehicleStyle.Overbike:
                    speedMultiplier = 1.85f;
                    canFly = true;
                    break;
                case VehicleStyle.Overwing:
                    speedMultiplier = 1.45f;
                    canFly = true;
                    break;
                default:
                    speedMultiplier = 1.7f;
                    canFly = true;
                    break;
            }
        }

        private void Awake()
        {
            localStart = transform.localPosition;
        }

        private void Update()
        {
            transform.localPosition = localStart + Vector3.up * Mathf.Sin(Time.time * hoverRate) * hoverBob;
        }
    }
}
