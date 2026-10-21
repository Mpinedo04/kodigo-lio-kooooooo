using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Blinks the body while the warrior recovers from a hit (post-hit invulnerability).
    [RequireComponent(typeof(Health))]
    public sealed class DamageFlicker : MonoBehaviour
    {
        [SerializeField] private float blinksPerSecond = 12f;

        private readonly List<Renderer> renderers = new List<Renderer>();
        private Health health;
        private bool hidden;

        private void Awake()
        {
            health = GetComponent<Health>();
            foreach (Renderer child in GetComponentsInChildren<Renderer>(true))
            {
                // Vehicles mount later under VehicleSocket and keep their own visibility.
                if (child.GetComponentInParent<VehicleRig>() == null)
                {
                    renderers.Add(child);
                }
            }
        }

        private void Update()
        {
            bool shouldHide = health.IsRecovering && Mathf.FloorToInt(Time.time * blinksPerSecond * 2f) % 2 == 0;
            if (shouldHide != hidden)
            {
                SetVisible(!shouldHide);
            }
        }

        private void OnDisable()
        {
            if (hidden)
            {
                SetVisible(true);
            }
        }

        private void SetVisible(bool visible)
        {
            hidden = !visible;
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }
        }
    }
}
