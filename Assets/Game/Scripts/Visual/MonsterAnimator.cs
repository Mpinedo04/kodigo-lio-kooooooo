using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Small life touches for the monsters (F9.16): legs step in diagonal pairs while they walk and wings flap.
    // The body itself (bobbing, rolling, opening) is moved by each monster's behaviour in Enemies/.
    public sealed class MonsterAnimator : MonoBehaviour
    {
        [SerializeField] private float stepSpeed = 4f;
        [SerializeField] private float stepAngle = 18f;
        [SerializeField] private float wingSpeed = 32f;
        [SerializeField] private float wingAngle = 28f;

        private readonly List<Transform> legs = new List<Transform>();
        private readonly List<Quaternion> legRest = new List<Quaternion>();
        private readonly List<Transform> wings = new List<Transform>();
        private readonly List<Quaternion> wingRest = new List<Quaternion>();
        private Vector3 lastPosition;
        private float phase;
        private float speed;

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Leg_"))
                {
                    legs.Add(child);
                    legRest.Add(child.localRotation);
                }
                else if (child.name.StartsWith("Wing_"))
                {
                    wings.Add(child);
                    wingRest.Add(child.localRotation);
                }
            }

            lastPosition = transform.position;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
            {
                return;
            }

            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            float flat = new Vector2(delta.x, delta.z).magnitude / dt;
            speed = Mathf.Lerp(speed, flat, 1f - Mathf.Exp(-8f * dt));
            float move01 = Mathf.Clamp01(speed / 2f);
            phase += speed * stepSpeed * dt;

            for (int i = 0; i < legs.Count; i++)
            {
                if (legs[i] == null || !legs[i].gameObject.activeSelf)
                {
                    continue;
                }

                // Front-left with back-right, front-right with back-left.
                bool firstPair = (i == 0 || i == 3);
                float swing = Mathf.Sin(phase + (firstPair ? 0f : Mathf.PI)) * stepAngle * move01;
                legs[i].localRotation = legRest[i] * Quaternion.Euler(swing, 0f, 0f);
            }

            float flap = Mathf.Sin(Time.time * wingSpeed) * wingAngle;
            for (int i = 0; i < wings.Count; i++)
            {
                if (wings[i] == null)
                {
                    continue;
                }

                float side = wings[i].localPosition.x < 0f ? 1f : -1f;
                wings[i].localRotation = wingRest[i] * Quaternion.Euler(0f, 0f, flap * side);
            }
        }
    }
}
