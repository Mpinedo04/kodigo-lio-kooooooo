using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Lock-on: picks the enemy closest to the centre of the screen, switches with a flick of
    // the right stick / mouse, and lets go when the target dies or gets too far.
    public sealed class LockOnSystem : MonoBehaviour
    {
        private static readonly Collider[] Buffer = new Collider[64];

        [SerializeField] private float maxDistance = 35f;
        [SerializeField] private LayerMask enemyMask = Physics.AllLayers;
        [SerializeField] private InputReader input;
        [SerializeField] private float switchCooldown = 0.35f;
        [Tooltip("Horizontal mouse travel (look units, about 10 px each) within the window that counts as a flick.")]
        [SerializeField] private float mouseFlickDistance = 12f;
        [SerializeField] private float mouseFlickWindow = 0.15f;
        [SerializeField] private float stickSwitchThreshold = 0.7f;
        [Tooltip("The stick must come back below this before another switch.")]
        [SerializeField] private float stickRearmThreshold = 0.3f;

        private readonly List<EnemyAI> enemies = new List<EnemyAI>();
        private readonly List<Vector3> positions = new List<Vector3>();
        private Transform owner;
        private Camera viewCamera;
        private float nextSwitchTime;
        private bool stickArmed = true;
        private float mouseAccumulated;
        private float mouseWindowStart;

        public Transform CurrentTarget { get; private set; }

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            CurrentTarget = null;
        }

        public void ToggleLock()
        {
            if (CurrentTarget != null || owner == null)
            {
                CurrentTarget = null;
                return;
            }

            CollectEnemies();
            Camera cam = ViewCamera;
            int best = cam != null
                ? TargetingMath.PickBest(cam.transform.position, cam.transform.forward, owner.position, positions)
                : PickClosest();
            CurrentTarget = best >= 0 ? AimPointOf(enemies[best]) : null;
            nextSwitchTime = Time.time + switchCooldown;
            stickArmed = false;
            mouseAccumulated = 0f;
        }

        public void CycleTarget(int direction)
        {
            Camera cam = ViewCamera;
            if (CurrentTarget == null || cam == null)
            {
                return;
            }

            CollectEnemies();
            int next = TargetingMath.PickNext(cam.transform.position, CurrentTarget.position, positions, direction);
            if (next >= 0)
            {
                CurrentTarget = AimPointOf(enemies[next]);
            }
        }

        private Camera ViewCamera
        {
            get
            {
                if (viewCamera == null)
                {
                    viewCamera = Camera.main;
                }

                return viewCamera;
            }
        }

        private void Update()
        {
            if (CurrentTarget == null)
            {
                return;
            }

            Health health = CurrentTarget.GetComponentInParent<Health>();
            if (health == null || health.IsDead || !CurrentTarget.gameObject.activeInHierarchy || owner == null
                || Vector3.Distance(owner.position, CurrentTarget.position) > maxDistance)
            {
                CurrentTarget = null;
                return;
            }

            if (input == null)
            {
                return;
            }

            int direction = input.LookFromGamepad ? ReadStickFlick(input.LookRaw.x) : ReadMouseFlick(input.LookRaw.x);
            if (direction != 0 && Time.time >= nextSwitchTime)
            {
                nextSwitchTime = Time.time + switchCooldown;
                CycleTarget(direction);
            }
        }

        // One switch per push: the stick has to return near the centre before the next one.
        private int ReadStickFlick(float x)
        {
            if (Mathf.Abs(x) < stickRearmThreshold)
            {
                stickArmed = true;
                return 0;
            }

            if (!stickArmed || Mathf.Abs(x) < stickSwitchThreshold)
            {
                return 0;
            }

            stickArmed = false;
            return x > 0f ? 1 : -1;
        }

        // Mouse travel accumulated over a short window, so it behaves the same at any frame rate.
        private int ReadMouseFlick(float x)
        {
            if (Time.time - mouseWindowStart > mouseFlickWindow || Mathf.Sign(x) != Mathf.Sign(mouseAccumulated))
            {
                mouseAccumulated = 0f;
                mouseWindowStart = Time.time;
            }

            mouseAccumulated += x;
            if (Mathf.Abs(mouseAccumulated) < mouseFlickDistance)
            {
                return 0;
            }

            int direction = mouseAccumulated > 0f ? 1 : -1;
            mouseAccumulated = 0f;
            return direction;
        }

        private void CollectEnemies()
        {
            enemies.Clear();
            positions.Clear();
            if (owner == null)
            {
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(owner.position, maxDistance, Buffer, enemyMask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                EnemyAI enemy = Buffer[i].GetComponentInParent<EnemyAI>();
                if (enemy == null || enemies.Contains(enemy))
                {
                    continue;
                }

                Health health = enemy.GetComponent<Health>();
                if (health != null && health.IsDead)
                {
                    continue;
                }

                enemies.Add(enemy);
                positions.Add(AimPointOf(enemy).position);
            }
        }

        private int PickClosest()
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < positions.Count; i++)
            {
                float distance = Vector3.SqrMagnitude(positions[i] - owner.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        private static Transform AimPointOf(EnemyAI enemy)
        {
            return enemy.AimPoint != null ? enemy.AimPoint : enemy.transform;
        }
    }
}
