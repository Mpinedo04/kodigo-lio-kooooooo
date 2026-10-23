using System;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Floating gate on the vehicle track. Lights up when a warrior passes through; the race mode will listen to Passed.
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrackGate : MonoBehaviour
    {
        [SerializeField] private int index;
        [SerializeField] private Renderer[] frame = new Renderer[0];
        [SerializeField] private Color passedColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private float glowSeconds = 1.5f;

        private Color[] idleColors;
        private float glowUntil = -1f;

        public static event Action<TrackGate, PlayerController> Passed;

        public int Index => index;

        public void Configure(int gateIndex, Renderer[] renderers)
        {
            index = gateIndex;
            frame = renderers;
        }

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            idleColors = new Color[frame.Length];
            for (int i = 0; i < frame.Length; i++)
            {
                idleColors[i] = frame[i] != null ? frame[i].material.color : Color.white;
            }
        }

        private void Update()
        {
            if (glowUntil >= 0f && Time.time >= glowUntil)
            {
                glowUntil = -1f;
                for (int i = 0; i < frame.Length; i++)
                {
                    SetColor(frame[i], idleColors[i]);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerController warrior = other.GetComponentInParent<PlayerController>();
            if (warrior == null)
            {
                return;
            }

            glowUntil = Time.time + glowSeconds;
            for (int i = 0; i < frame.Length; i++)
            {
                SetColor(frame[i], passedColor);
            }

            Passed?.Invoke(this, warrior);
        }

        private static void SetColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.material.color = color;
            if (renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", color * 1.4f);
            }
        }
    }
}
