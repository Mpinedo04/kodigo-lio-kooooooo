using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Team combos that only exist when two warriors fight together (F5.25-F5.26, Biblia 13.2):
    // Yumi + Ulrich (finish a lifted monster), Odd + Aelita (shielded double energy field),
    // Yumi + Aelita (huge Creativity) and Ulrich + William (double heavy blow breaks armour).
    public static class TeamSynergy
    {
        private struct HeavyHit
        {
            public LyokoCharacter attacker;
            public float time;
        }

        private static readonly Dictionary<Health, HeavyHit> LastHeavyHits = new Dictionary<Health, HeavyHit>();
        private static SquadProfile profile;
        private static bool applyingBonus;

        public static string LastSynergy { get; private set; }
        public static float LastSynergyTime { get; private set; } = float.NegativeInfinity;

        private static SquadProfile Profile
        {
            get
            {
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<SquadProfile>();
                }

                return profile;
            }
        }

        public static void SetProfile(SquadProfile squadProfile)
        {
            if (squadProfile != null)
            {
                profile = squadProfile;
            }
        }

        public static bool AreClose(Vector3 a, Vector3 b, float distance)
        {
            return (a - b).sqrMagnitude <= distance * distance;
        }

        // A warrior of that character still in Lyoko (controlled or companion).
        public static PlayerController Find(LyokoCharacter character)
        {
            IReadOnlyList<PlayerController> all = PlayerController.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].Character != character)
                {
                    continue;
                }

                Health health = all[i].GetComponent<Health>();
                if (health == null || !health.IsDead)
                {
                    return all[i];
                }
            }

            return null;
        }

        // Odd + Aelita: behind Odd's raised shield, Aelita's energy fields are doubled.
        public static float EnergyFieldMultiplier(Vector3 aelitaPosition)
        {
            PlayerController odd = Find(LyokoCharacter.Odd);
            if (odd == null || !AreClose(odd.transform.position, aelitaPosition, Profile.oddAelitaDistance))
            {
                return 1f;
            }

            AbilityController abilities = odd.GetComponent<AbilityController>();
            OddShieldAbility shield = abilities != null ? abilities.Find<OddShieldAbility>() : null;
            if (shield == null || !shield.IsActive)
            {
                return 1f;
            }

            Announce("Odd + Aelita: campo de energia doble");
            return Profile.energyFieldMultiplier;
        }

        // Yumi + Aelita: Creativity with Yumi's telekinesis close by makes huge structures.
        public static bool CreativityBoost(Vector3 aelitaPosition, out float sizeMultiplier, out float durationMultiplier)
        {
            PlayerController yumi = Find(LyokoCharacter.Yumi);
            bool boosted = yumi != null && AreClose(yumi.transform.position, aelitaPosition, Profile.yumiAelitaDistance);
            sizeMultiplier = boosted ? Profile.hugeStructureSize : 1f;
            durationMultiplier = boosted ? Profile.hugeStructureDuration : 1f;
            if (boosted)
            {
                Announce("Yumi + Aelita: Creatividad gigante");
            }

            return boosted;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Health.AnyDamaged -= OnAnyDamaged;
            LastHeavyHits.Clear();
            applyingBonus = false;
            LastSynergy = null;
            LastSynergyTime = float.NegativeInfinity;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Hook()
        {
            Health.AnyDamaged -= OnAnyDamaged;
            Health.AnyDamaged += OnAnyDamaged;
        }

        private static void OnAnyDamaged(Health target, DamageInfo info, float applied)
        {
            if (applyingBonus || applied <= 0f || target == null || target.Team != Team.Enemy || target.IsDead || info.source == null)
            {
                return;
            }

            PlayerController attacker = info.source.GetComponent<PlayerController>();
            if (attacker == null)
            {
                return;
            }

            // Yumi + Ulrich: Yumi lifts a monster and Ulrich finishes it in the air.
            if (attacker.Character == LyokoCharacter.Ulrich && info.type == DamageType.Melee && TelekinesisState.IsLifted(target.gameObject))
            {
                PlayerController yumi = Find(LyokoCharacter.Yumi);
                if (yumi != null && AreClose(yumi.transform.position, attacker.transform.position, Profile.yumiUlrichDistance))
                {
                    Bonus(target, info, applied * Profile.liftedHitBonus, "Yumi + Ulrich: remate en el aire");
                }
            }

            // Ulrich + William: two heavy blows on the same monster almost at once crack its armour.
            bool heavyHitter = attacker.Character == LyokoCharacter.Ulrich || attacker.Character == LyokoCharacter.William;
            if (info.isHeavy && heavyHitter && !target.IsDead)
            {
                if (LastHeavyHits.TryGetValue(target, out HeavyHit previous) && previous.attacker != attacker.Character && Time.time - previous.time <= Profile.doubleHeavyWindow)
                {
                    LastHeavyHits.Remove(target);
                    EnemyAI enemy = target.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.ExposeWeakPoint(Profile.armourBreakSeconds);
                    }

                    Bonus(target, info, applied * Profile.doubleHeavyBonus, "Ulrich + William: armadura rota");
                }
                else
                {
                    LastHeavyHits[target] = new HeavyHit { attacker = attacker.Character, time = Time.time };
                }
            }
        }

        private static void Bonus(Health target, DamageInfo info, float amount, string label)
        {
            if (amount <= 0f || target.IsDead)
            {
                return;
            }

            DamageInfo bonus = info;
            bonus.amount = amount;
            bonus.type = DamageType.Ability;
            bonus.isCritical = true;
            bonus.knockback = 0f;
            bonus.ignoreInvulnerability = true;

            applyingBonus = true;
            target.TakeDamage(bonus);
            applyingBonus = false;
            Announce(label);
        }

        private static void Announce(string label)
        {
            if (label != LastSynergy || Time.unscaledTime - LastSynergyTime > 1.5f)
            {
                Debug.Log("[Synergy] " + label);
            }

            LastSynergy = label;
            LastSynergyTime = Time.unscaledTime;
        }
    }
}
