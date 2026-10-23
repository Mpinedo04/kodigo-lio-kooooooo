using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CodeLyokoFanGame
{
    // Scenery Yumi can move with telekinesis (F5.20): rocks to throw, blocks for puzzles.
    // Released hard, it hurts the monsters it crashes into.
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TelekinesisTarget : MonoBehaviour
    {
        private static readonly List<TelekinesisTarget> Active = new List<TelekinesisTarget>();

        [Tooltip("Minimum crash speed that hurts a monster.")]
        [SerializeField] private float impactSpeed = 6f;
        [SerializeField] private float followSharpness = 10f;

        private readonly HashSet<Health> hitThisThrow = new HashSet<Health>();
        private Rigidbody body;
        private NavMeshObstacle obstacle;
        private GameObject thrower;
        private float thrownDamage;
        private float thrownUntil;

        public event Action<TelekinesisTarget> Released;

        public static IReadOnlyList<TelekinesisTarget> All => Active;

        public bool IsHeld { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            obstacle = GetComponent<NavMeshObstacle>();
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        public void BeginLift()
        {
            IsHeld = true;
            body.isKinematic = true;
            if (obstacle != null)
            {
                obstacle.enabled = false;
            }
        }

        public void MoveTowards(Vector3 point, float dt)
        {
            body.MovePosition(Vector3.Lerp(body.position, point, 1f - Mathf.Exp(-followSharpness * dt)));
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, 40f * dt, 0f));
        }

        public void Release(Vector3 velocity, GameObject releasedBy, float damage)
        {
            IsHeld = false;
            body.isKinematic = false;
            body.velocity = velocity;
            thrower = releasedBy;
            thrownDamage = damage;
            thrownUntil = Time.time + 3f;
            hitThisThrow.Clear();
            if (obstacle != null)
            {
                obstacle.enabled = true;
            }

            Released?.Invoke(this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time > thrownUntil || collision.contactCount == 0 || collision.relativeVelocity.magnitude < impactSpeed)
            {
                return;
            }

            DamageInfo crash = new DamageInfo
            {
                amount = thrownDamage,
                type = DamageType.Ability,
                source = thrower,
                sourceTeam = Team.Player,
                point = collision.GetContact(0).point,
                direction = body.velocity,
                knockback = 6f,
                isHeavy = true
            };

            if (HitResolver.ApplyToCollider(collision.collider, crash, Team.Player, hitThisThrow) != null)
            {
                CameraRig.Shake(0.3f);
            }
        }
    }
}
