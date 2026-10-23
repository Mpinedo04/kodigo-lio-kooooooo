using System;
using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Keeps fights fair: limits how many monsters attack at once and shares targets between the
    // warriors, AI companions, Aelita and clones (any alive Player-team Health; see AggroTarget).
    // One per scene; created on demand when a level does not place one.
    public sealed class CombatDirector : MonoBehaviour
    {
        private static CombatDirector instance;
        private static CombatDirectorProfile fallbackProfile;

        [SerializeField] private CombatDirectorProfile profile;

        private readonly AttackSlots slots = new AttackSlots();
        private readonly Dictionary<EnemyAI, Health> engagements = new Dictionary<EnemyAI, Health>();
        private readonly Dictionary<Health, float> lastAttackOn = new Dictionary<Health, float>();
        private readonly Dictionary<EnemyAI, float> waitingSince = new Dictionary<EnemyAI, float>();
        private readonly Dictionary<EnemyAI, float> lastRequest = new Dictionary<EnemyAI, float>();
        private readonly List<EnemyAI> scratch = new List<EnemyAI>();
        private CharacterSwitcher switcher;
        private float switcherLookupTimer;

        // Null outside Play mode (edit-mode tests run monsters without a director).
        public static CombatDirector Instance
        {
            get
            {
                if (instance == null && Application.isPlaying)
                {
                    instance = FindObjectOfType<CombatDirector>();
                    if (instance == null)
                    {
                        instance = new GameObject("CombatDirector").AddComponent<CombatDirector>();
                    }
                }

                return instance;
            }
        }

        // The director of the scene if there is one; never creates it (safe while unloading).
        public static CombatDirector Current => instance;

        public static CombatDirectorProfile DefaultProfile
        {
            get
            {
                if (fallbackProfile == null)
                {
                    fallbackProfile = ScriptableObject.CreateInstance<CombatDirectorProfile>();
                    fallbackProfile.hideFlags = HideFlags.DontSave;
                }

                return fallbackProfile;
            }
        }

        public CombatDirectorProfile Profile => profile != null ? profile : DefaultProfile;
        public int ActiveAttackers => slots.Count;

        public void SetProfile(CombatDirectorProfile value)
        {
            profile = value;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            slots.Prune(key => key as EnemyAI == null);
            scratch.Clear();
            foreach (KeyValuePair<EnemyAI, Health> pair in engagements)
            {
                if (pair.Key == null || pair.Value == null || pair.Value.IsDead)
                {
                    scratch.Add(pair.Key);
                }
            }

            for (int i = 0; i < scratch.Count; i++)
            {
                engagements.Remove(scratch[i]);
            }

            if (lastAttackOn.Count > 16)
            {
                lastAttackOn.Clear();
            }

            // Requests that stopped (target lost, monster busy) leave the queue.
            scratch.Clear();
            foreach (KeyValuePair<EnemyAI, float> pair in lastRequest)
            {
                if (pair.Key == null || Time.time - pair.Value > 1f)
                {
                    scratch.Add(pair.Key);
                }
            }

            for (int i = 0; i < scratch.Count; i++)
            {
                waitingSince.Remove(scratch[i]);
                lastRequest.Remove(scratch[i]);
            }
        }

        // ---- Targets ------------------------------------------------------------------------------

        public Health ChooseTarget(EnemyAI enemy, Health current, float range, Predicate<Health> canPerceive)
        {
            Health best = SelectTarget(Health.All, enemy.transform.position, current, range, canPerceive, Profile, ControlledWarrior(),
                target => EngagedCount(target, enemy));
            Engage(enemy, best != null ? best : current);
            return best;
        }

        // Pure choice used by the director and by monsters without one.
        public static Health SelectTarget(IReadOnlyList<Health> candidates, Vector3 from, Health current, float range,
            Predicate<Health> canPerceive, CombatDirectorProfile settings, Health controlled, Func<Health, int> engagedByOthers)
        {
            Health best = null;
            float bestScore = 0f;
            float currentScore = 0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                Health candidate = candidates[i];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                bool isCurrent = candidate == current;
                float distance = Vector3.Distance(from, candidate.transform.position);
                float maxRange = isCurrent ? range * settings.keepTargetRangeMultiplier : range;
                if (distance > maxRange || (!isCurrent && canPerceive != null && !canPerceive(candidate)))
                {
                    continue;
                }

                int engaged = engagedByOthers != null ? engagedByOthers(candidate) : 0;
                float score = Score(AggroTarget.PriorityOf(candidate), distance, engaged, settings, candidate == controlled);
                if (isCurrent)
                {
                    currentScore = score;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            if (current != null && currentScore > 0f && best != current && bestScore < currentScore * settings.switchMargin)
            {
                return current;
            }

            return best;
        }

        public static float Score(float priority, float distance, int engagedByOthers, CombatDirectorProfile settings, bool controlled)
        {
            return Score(priority, distance, engagedByOthers, settings.distanceFalloff, settings.crowdThreshold, settings.crowdPenalty,
                controlled ? settings.controlledWarriorBonus : 1f);
        }

        public static float Score(float priority, float distance, int engagedByOthers, float distanceFalloff, int crowdThreshold, float crowdPenalty, float bonus)
        {
            if (priority <= 0f)
            {
                return 0f;
            }

            float falloff = Mathf.Max(0.01f, distanceFalloff);
            float score = priority * bonus * falloff / (falloff + Mathf.Max(0f, distance));
            int crowding = engagedByOthers - Mathf.Max(1, crowdThreshold) + 1;
            if (crowding > 0)
            {
                score /= 1f + crowdPenalty * crowding;
            }

            return score;
        }

        public static bool IsValidTarget(Health health)
        {
            return health != null && health.Team == Team.Player && !health.IsDead && health.gameObject.activeInHierarchy
                && AggroTarget.PriorityOf(health) > 0f;
        }

        public int EngagedCount(Health target, EnemyAI except)
        {
            int count = 0;
            foreach (KeyValuePair<EnemyAI, Health> pair in engagements)
            {
                if (pair.Value == target && pair.Key != except && pair.Key != null)
                {
                    count++;
                }
            }

            return count;
        }

        // Kankrelats spread around their target: each one gets a slot on a ring.
        public int RingSlot(EnemyAI enemy, Health target, out int count, out Vector3 anchorDirection)
        {
            scratch.Clear();
            foreach (KeyValuePair<EnemyAI, Health> pair in engagements)
            {
                if (pair.Value == target && pair.Key != null && pair.Key.Kind == enemy.Kind)
                {
                    scratch.Add(pair.Key);
                }
            }

            if (!scratch.Contains(enemy))
            {
                scratch.Add(enemy);
            }

            scratch.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
            count = scratch.Count;
            anchorDirection = Vector3.ProjectOnPlane(scratch[0].transform.position - target.transform.position, Vector3.up);
            return scratch.IndexOf(enemy);
        }

        public void Forget(EnemyAI enemy)
        {
            engagements.Remove(enemy);
            waitingSince.Remove(enemy);
            lastRequest.Remove(enemy);
            slots.Release(enemy);
        }

        // ---- Attack slots -------------------------------------------------------------------------

        // Fair turns: the monster that has been waiting longest for the same target goes first,
        // so fast attackers (Kankrelats) cannot starve slow ones (Megatank).
        public bool TryBeginAttack(EnemyAI enemy, Health target)
        {
            float now = Time.time;
            if (!waitingSince.ContainsKey(enemy))
            {
                waitingSince[enemy] = now;
            }

            lastRequest[enemy] = now;
            if (target != null && lastAttackOn.TryGetValue(target, out float last) && now - last < Profile.minSecondsBetweenAttacksOnSameTarget)
            {
                return false;
            }

            if (target != null && SomeoneWaitedLonger(enemy, target, now))
            {
                return false;
            }

            if (!slots.TryAcquire(enemy, target, Profile.maxSimultaneousAttackers, Profile.maxAttackersPerTarget))
            {
                return false;
            }

            waitingSince.Remove(enemy);
            if (target != null)
            {
                lastAttackOn[target] = now;
            }

            return true;
        }

        private bool SomeoneWaitedLonger(EnemyAI enemy, Health target, float now)
        {
            float mine = waitingSince[enemy];
            foreach (KeyValuePair<EnemyAI, float> pair in waitingSince)
            {
                EnemyAI other = pair.Key;
                if (other == enemy || other == null || pair.Value >= mine)
                {
                    continue;
                }

                bool stillAsking = lastRequest.TryGetValue(other, out float asked) && now - asked <= 0.25f;
                if (stillAsking && engagements.TryGetValue(other, out Health otherTarget) && otherTarget == target)
                {
                    return true;
                }
            }

            return false;
        }

        public void EndAttack(EnemyAI enemy)
        {
            slots.Release(enemy);
        }

        public bool IsAttacking(EnemyAI enemy)
        {
            return slots.Holds(enemy);
        }

        public void Engage(EnemyAI enemy, Health target)
        {
            if (target == null)
            {
                engagements.Remove(enemy);
            }
            else
            {
                engagements[enemy] = target;
            }
        }

        private Health ControlledWarrior()
        {
            switcherLookupTimer -= Time.deltaTime;
            if (switcher == null && switcherLookupTimer <= 0f)
            {
                switcherLookupTimer = 1f;
                switcher = FindObjectOfType<CharacterSwitcher>();
            }

            PlayerController active = switcher != null ? switcher.ActiveCharacter : null;
            return active != null ? active.GetComponent<Health>() : null;
        }
    }
}
