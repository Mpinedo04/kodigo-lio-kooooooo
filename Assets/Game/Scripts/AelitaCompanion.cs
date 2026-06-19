using UnityEngine;

namespace CodeLyokoFanGame
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AelitaCompanion : MonoBehaviour
    {
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private float followDistance = 3.4f;
        [SerializeField] private float moveSpeed = 5.2f;
        [SerializeField] private float shieldRadius = 5f;
        [SerializeField] private float shieldCooldown = 8f;

        private CharacterController controller;
        private float shieldTimer;

        public bool IsNearTower(TowerObjective tower)
        {
            return tower != null && Vector3.Distance(transform.position, tower.transform.position) <= tower.RequiredAelitaDistance;
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            shieldTimer -= Time.deltaTime;

            if (switcher == null || switcher.ActiveCharacter == null)
            {
                return;
            }

            Transform target = switcher.ActiveCharacter.transform;
            Vector3 toTarget = target.position - transform.position;
            Vector3 flat = Vector3.ProjectOnPlane(toTarget, Vector3.up);

            if (flat.magnitude > followDistance)
            {
                controller.Move(flat.normalized * moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat.normalized), Time.deltaTime * 8f);
            }

            controller.Move(Vector3.down * 12f * Time.deltaTime);

            if (shieldTimer <= 0f)
            {
                Collider[] enemies = Physics.OverlapSphere(transform.position, shieldRadius);
                for (int i = 0; i < enemies.Length; i++)
                {
                    EnemyAI enemy = enemies[i].GetComponentInParent<EnemyAI>();
                    if (enemy != null)
                    {
                        Health health = enemy.GetComponent<Health>();
                        if (health != null)
                        {
                            health.TakeDamage(10f, gameObject);
                        }
                    }
                }

                shieldTimer = shieldCooldown;
            }
        }
    }
}
