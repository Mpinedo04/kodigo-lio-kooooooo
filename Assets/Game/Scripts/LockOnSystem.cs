using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class LockOnSystem : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 35f;
        [SerializeField] private LayerMask enemyMask = ~0;

        private Transform owner;

        public Transform CurrentTarget { get; private set; }

        public void SetOwner(Transform newOwner)
        {
            owner = newOwner;
            CurrentTarget = null;
        }

        public void ToggleLock()
        {
            if (CurrentTarget != null)
            {
                CurrentTarget = null;
                return;
            }

            CurrentTarget = FindClosestEnemy();
        }

        private void Update()
        {
            if (CurrentTarget == null)
            {
                return;
            }

            Health health = CurrentTarget.GetComponentInParent<Health>();
            if (health == null || health.IsDead || owner == null || Vector3.Distance(owner.position, CurrentTarget.position) > maxDistance)
            {
                CurrentTarget = null;
            }
        }

        private Transform FindClosestEnemy()
        {
            if (owner == null)
            {
                return null;
            }

            Collider[] candidates = Physics.OverlapSphere(owner.position, maxDistance, enemyMask, QueryTriggerInteraction.Collide);
            float bestDistance = float.MaxValue;
            Transform best = null;

            for (int i = 0; i < candidates.Length; i++)
            {
                EnemyAI enemy = candidates[i].GetComponentInParent<EnemyAI>();
                if (enemy == null)
                {
                    continue;
                }

                float distance = Vector3.SqrMagnitude(enemy.transform.position - owner.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy.AimPoint != null ? enemy.AimPoint : enemy.transform;
                }
            }

            return best;
        }
    }
}
