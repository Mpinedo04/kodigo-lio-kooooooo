using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Floating damage numbers (yellow = weak point). Lives in Boot; can be turned off in the options.
    public sealed class DamageNumbers : MonoBehaviour
    {
        private struct Entry
        {
            public Vector3 position;
            public string text;
            public bool critical;
            public bool againstPlayer;
            public float age;
        }

        [SerializeField] private float lifetime = 0.9f;
        [SerializeField] private float riseSpeed = 1.5f;

        private readonly List<Entry> entries = new List<Entry>();
        private GUIStyle style;

        private void OnEnable()
        {
            Health.AnyDamaged += OnAnyDamaged;
        }

        private void OnDisable()
        {
            Health.AnyDamaged -= OnAnyDamaged;
        }

        private void OnAnyDamaged(Health health, DamageInfo info, float applied)
        {
            if (!GameSettings.ShowDamageNumbers || applied < 0.5f || info.type == DamageType.Fall)
            {
                return;
            }

            entries.Add(new Entry
            {
                position = health.transform.position + Vector3.up * 2.4f + Random.insideUnitSphere * 0.3f,
                text = Mathf.RoundToInt(applied).ToString(),
                critical = info.isCritical,
                againstPlayer = health.Team == Team.Player
            });
        }

        private void Update()
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                Entry entry = entries[i];
                entry.age += Time.unscaledDeltaTime;
                entry.position += Vector3.up * riseSpeed * Time.unscaledDeltaTime;
                if (entry.age >= lifetime)
                {
                    entries.RemoveAt(i);
                }
                else
                {
                    entries[i] = entry;
                }
            }
        }

        private void OnGUI()
        {
            Camera cam = Camera.main;
            if (cam == null || entries.Count == 0)
            {
                return;
            }

            if (style == null)
            {
                style = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            }

            for (int i = 0; i < entries.Count; i++)
            {
                Vector3 screen = cam.WorldToScreenPoint(entries[i].position);
                if (screen.z <= 0f)
                {
                    continue;
                }

                float alpha = 1f - entries[i].age / lifetime;
                Color color = entries[i].againstPlayer ? new Color(1f, 0.3f, 0.3f) : entries[i].critical ? new Color(1f, 0.9f, 0.2f) : Color.white;
                style.normal.textColor = new Color(color.r, color.g, color.b, alpha);
                style.fontSize = entries[i].critical ? 26 : 20;
                GUI.Label(new Rect(screen.x - 40f, Screen.height - screen.y - 15f, 80f, 30f), entries[i].text, style);
            }
        }
    }
}
