using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Seed values for EnemyDefinition assets. The editor generator (EnemyDataBuilder) writes them
    // into the assets, which are what the game reads; at runtime this is only a fallback for
    // monsters spawned without an asset. Season 1 values follow the Biblia (section 8.1).
    public static class EnemyDefaults
    {
        public static bool IsSeasonOne(EnemyKind kind)
        {
            return kind == EnemyKind.Kankrelat || kind == EnemyKind.Blok || kind == EnemyKind.Krab
                || kind == EnemyKind.Hornet || kind == EnemyKind.Megatank;
        }

        public static EnemyDefinition CreateRuntime(EnemyKind kind)
        {
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.hideFlags = HideFlags.DontSave;
            definition.name = kind + "_RuntimeDefaults";
            definition.kind = kind;
            definition.displayName = kind.ToString();
            ApplyLegacy(definition, kind);
            if (IsSeasonOne(kind))
            {
                ApplySeasonOne(definition);
            }

            return definition;
        }

        public static void ApplySeasonOne(EnemyDefinition d)
        {
            switch (d.kind)
            {
                case EnemyKind.Kankrelat:
                    Kankrelat(d);
                    break;
                case EnemyKind.Blok:
                    Blok(d);
                    break;
                case EnemyKind.Krab:
                    Krab(d);
                    break;
                case EnemyKind.Hornet:
                    Hornet(d);
                    break;
                case EnemyKind.Megatank:
                    Megatank(d);
                    break;
            }
        }

        // Small cockroach in groups of 3-6: weak laser, dies from one hit on the eye.
        private static void Kankrelat(EnemyDefinition d)
        {
            Common(d, EnemyBehaviorStyle.SwarmShooter, 20f, 10f, 20f, 14f, 4.2f);
            d.canonNotes = "Cucaracha pequena. Laser desde el ojo. Ojo arriba; va en grupos. PV 20, dano 10, amenaza 1.";
            d.visionAngle = 220f;
            d.alertAllyRadius = 14f;
            d.reactionTime = 0.3f;
            d.preferredDistance = 7f;
            d.turnSpeed = 540f;
            d.bodyRadius = 0.6f;
            d.attackGap = 0.8f;
            d.flying = false;
            d.hoverHeight = 0f;
            WeakPoint(d, true, true, 1f);
            Reactions(d, 1f, 0.6f, 0f, true);
            d.dataDrop = 1;
            d.attacks = new List<EnemyAttackDefinition>
            {
                Attack("Laser", EnemyAttackType.Laser, 10f, 2.2f, 14f, 0.45f, 0.3f)
            };
        }

        // Cube turret: four shots, turns so the eye is never facing the warrior. Flank it.
        private static void Blok(EnemyDefinition d)
        {
            Common(d, EnemyBehaviorStyle.Turret, 50f, 10f, 24f, 18f, 0f);
            d.canonNotes = "Cubo con ojos. Laser, laser congelante, anillos de fuego y rayo prision. Hay que flanquear. PV 50, dano 10-20, amenaza 2.";
            d.visionAngle = 360f;
            d.alertAllyRadius = 12f;
            d.reactionTime = 0.5f;
            d.patrolRadius = 0f;
            d.preferredDistance = 0f;
            d.turnSpeed = 80f;
            d.bodyRadius = 0.85f;
            d.attackGap = 1.2f;
            WeakPoint(d, true, false, 0.5f);
            Reactions(d, 1f, 0.4f, 1.6f, false);
            d.dataDrop = 2;

            EnemyAttackDefinition ice = Attack("Laser congelante", EnemyAttackType.IceShot, 10f, 6f, 16f, 0.8f, 0.4f);
            ice.slowMultiplier = 0.5f;
            ice.statusDuration = 2.5f;
            ice.projectileSpeed = 16f;

            EnemyAttackDefinition rings = Attack("Anillos de fuego", EnemyAttackType.FireRing, 15f, 7f, 8f, 1f, 0.6f);
            rings.burstCount = 3;
            rings.burstInterval = 0.45f;
            rings.activeTime = 1.3f;
            rings.areaSize = 9f;
            rings.areaThickness = 0.8f;
            rings.knockback = 4f;
            rings.weight = 2f;

            EnemyAttackDefinition prison = Attack("Rayo prision", EnemyAttackType.PrisonRay, 10f, 10f, 14f, 1f, 0.5f);
            prison.statusDuration = 1.5f;
            prison.projectileSpeed = 14f;

            d.attacks = new List<EnemyAttackDefinition>
            {
                Attack("Laser", EnemyAttackType.Laser, 10f, 2.5f, 18f, 0.6f, 0.4f),
                ice,
                rings,
                prison
            };
        }

        // Tall four-legged crab: laser and kicks. Break two legs to knock it down and expose the eye on top.
        private static void Krab(EnemyDefinition d)
        {
            Common(d, EnemyBehaviorStyle.HeavyWalker, 80f, 20f, 26f, 16f, 3f);
            d.canonNotes = "Cangrejo alto de cuatro patas. Laser y patadas. Ojo arriba; derribarlo atacando las patas. PV 80, dano 20, amenaza 3.";
            d.visionAngle = 160f;
            d.alertAllyRadius = 14f;
            d.reactionTime = 0.5f;
            d.preferredDistance = 9f;
            d.turnSpeed = 160f;
            d.bodyRadius = 1.2f;
            d.attackGap = 1f;
            WeakPoint(d, false, false, 0.5f);
            Reactions(d, 1f, 0.4f, 0f, false);
            d.partHealth = 25f;
            d.partsToKnockDown = 2;
            d.knockedDownDuration = 4f;
            d.dataDrop = 3;

            EnemyAttackDefinition kick = Attack("Patada", EnemyAttackType.Melee, 20f, 2f, 3.6f, 0.55f, 0.5f);
            kick.areaSize = 2.6f;
            kick.knockback = 9f;
            kick.heavy = true;
            kick.weight = 3f;

            d.attacks = new List<EnemyAttackDefinition>
            {
                Attack("Laser", EnemyAttackType.Laser, 20f, 2.8f, 16f, 0.7f, 0.5f),
                kick
            };
        }

        // Flying wasp: circles, dives to fire a burst, climbs again; flees when left alone.
        private static void Hornet(EnemyDefinition d)
        {
            Common(d, EnemyBehaviorStyle.FlyingShooter, 30f, 10f, 28f, 18f, 6f);
            d.canonNotes = "Avispa voladora (Frelion). Laser rapido. Ojo en la frente; mejor a distancia. PV 30, dano 10, amenaza 2.";
            d.flying = true;
            d.hoverHeight = 4.5f;
            d.visionAngle = 240f;
            d.alertAllyRadius = 16f;
            d.reactionTime = 0.3f;
            d.patrolRadius = 5f;
            d.preferredDistance = 9f;
            d.turnSpeed = 360f;
            d.attackGap = 1f;
            WeakPoint(d, true, true, 1f);
            Reactions(d, 1f, 0.6f, 0f, true);
            d.dataDrop = 1;

            EnemyAttackDefinition burst = Attack("Rafaga", EnemyAttackType.Burst, 10f, 3.5f, 18f, 0.7f, 0.6f);
            burst.burstCount = 3;
            burst.burstInterval = 0.18f;
            d.attacks = new List<EnemyAttackDefinition> { burst };
        }

        // Armoured rolling ball: stops, opens (1 s warning) and fires a horizontal or vertical beam.
        private static void Megatank(EnemyDefinition d)
        {
            Common(d, EnemyBehaviorStyle.ArmoredBurst, 150f, 50f, 30f, 20f, 3.2f);
            d.canonNotes = "Bola rodante blindada. Se abre y dispara un rayo enorme. Solo vulnerable abierta. PV 150, dano 40-60, amenaza 4.";
            d.boss = true;
            d.visionAngle = 180f;
            d.alertAllyRadius = 18f;
            d.reactionTime = 0.8f;
            d.patrolRadius = 3f;
            d.preferredDistance = 12f;
            d.turnSpeed = 110f;
            d.bodyRadius = 1.4f;
            d.attackGap = 1.5f;
            WeakPoint(d, false, false, 0f);
            Reactions(d, 0.3f, 0f, 1.2f, false);
            d.dataDrop = 5;

            EnemyAttackDefinition horizontal = Beam("Rayo horizontal", EnemyAttackType.BeamHorizontal, 40f, 7f, 0.8f);
            EnemyAttackDefinition vertical = Beam("Rayo vertical", EnemyAttackType.BeamVertical, 60f, 6f, 1.4f);
            d.attacks = new List<EnemyAttackDefinition> { horizontal, vertical };
        }

        private static EnemyAttackDefinition Beam(string name, EnemyAttackType type, float damage, float wide, float thin)
        {
            EnemyAttackDefinition beam = Attack(name, type, damage, 5f, 20f, 1f, 1f);
            beam.activeTime = 0.6f;
            beam.areaSize = 24f;
            beam.areaWidth = wide;
            beam.areaThickness = thin;
            beam.knockback = 10f;
            beam.heavy = true;
            return beam;
        }

        private static void Common(EnemyDefinition d, EnemyBehaviorStyle style, float health, float damage, float detection, float attackRange, float speed)
        {
            d.behaviorStyle = style;
            d.maxHealth = health;
            d.projectileDamage = damage;
            d.detectionRange = detection;
            d.attackRange = attackRange;
            d.moveSpeed = speed;
            d.contactDamage = 0f;
            d.forgetTargetAfter = 4f;
            d.idleTime = 2f;
            d.patrolRadius = 4f;
        }

        private static void WeakPoint(EnemyDefinition d, bool alwaysExposed, bool instantKill, float knockbackScale)
        {
            d.weakPointNotes = "Ojo de X.A.N.A.";
            d.weakPointDamageMultiplier = 2.5f;
            d.weakPointAlwaysExposed = alwaysExposed;
            d.weakPointInstantKill = instantKill;
            d.knockbackScale = knockbackScale;
        }

        private static void Reactions(EnemyDefinition d, float staggerMultiplier, float heavyHitStagger, float guardCrack, bool liftable)
        {
            d.staggerMultiplier = staggerMultiplier;
            d.heavyHitStagger = heavyHitStagger;
            d.guardCrackDuration = guardCrack;
            d.liftableByTelekinesis = liftable;
        }

        public static EnemyAttackDefinition Attack(string name, EnemyAttackType type, float damage, float cooldown, float range, float windup, float recovery)
        {
            return new EnemyAttackDefinition
            {
                attackName = name,
                type = type,
                damage = damage,
                cooldown = cooldown,
                range = range,
                windup = windup,
                recovery = recovery
            };
        }

        // The pre-F06 tables, kept for the monsters of later seasons until their own phase.
        private static void ApplyLegacy(EnemyDefinition d, EnemyKind kind)
        {
            float maxHealth = 60f;
            d.behaviorStyle = EnemyBehaviorStyle.SwarmShooter;
            d.weakPointAlwaysExposed = true;
            d.detectionRange = 22f;
            d.attackRange = 15f;
            d.moveSpeed = 3.8f;
            d.projectileDamage = 9f;
            d.fireCooldown = 0.95f;

            switch (kind)
            {
                case EnemyKind.Tarantula:
                    maxHealth = 120f;
                    d.behaviorStyle = EnemyBehaviorStyle.EliteShooter;
                    Ranges(d, 32f, 25f, 2.8f, 15f, 0.8f);
                    break;
                case EnemyKind.Creeper:
                    maxHealth = 60f;
                    d.behaviorStyle = EnemyBehaviorStyle.Ambusher;
                    Ranges(d, 22f, 12f, 5.2f, 20f, 1f);
                    d.contactDamage = 20f;
                    break;
                case EnemyKind.Manta:
                case EnemyKind.BlackManta:
                    maxHealth = kind == EnemyKind.BlackManta ? 120f : 90f;
                    d.behaviorStyle = EnemyBehaviorStyle.MineLayer;
                    Ranges(d, 34f, 25f, 4.2f, 20f, 1.1f);
                    d.hoverHeight = 5f;
                    d.flying = true;
                    break;
                case EnemyKind.Kongre:
                case EnemyKind.Shark:
                    maxHealth = kind == EnemyKind.Shark ? 70f : 40f;
                    d.behaviorStyle = EnemyBehaviorStyle.DigitalSeaChaser;
                    Ranges(d, 34f, 18f, 5f, kind == EnemyKind.Shark ? 20f : 10f, 1.1f);
                    d.hoverHeight = 2.6f;
                    d.flying = true;
                    break;
                case EnemyKind.Kalamar:
                    maxHealth = 250f;
                    d.behaviorStyle = EnemyBehaviorStyle.Grappler;
                    Ranges(d, 30f, 10f, 3.4f, 30f, 1.8f);
                    d.contactDamage = 30f;
                    d.hoverHeight = 3.2f;
                    d.flying = true;
                    break;
                case EnemyKind.Guardian:
                    maxHealth = 220f;
                    d.behaviorStyle = EnemyBehaviorStyle.CaptureField;
                    Ranges(d, 20f, 6f, 2.2f, 12f, 2f);
                    d.weakPointAlwaysExposed = false;
                    break;
                case EnemyKind.Scyphozoa:
                    maxHealth = 200f;
                    d.behaviorStyle = EnemyBehaviorStyle.MemoryDrainer;
                    Ranges(d, 30f, 5f, 2.6f, 20f, 1f);
                    d.hoverHeight = 4.2f;
                    d.flying = true;
                    d.weakPointAlwaysExposed = false;
                    break;
                case EnemyKind.Kolossus:
                    maxHealth = 1000f;
                    d.behaviorStyle = EnemyBehaviorStyle.RaidBoss;
                    Ranges(d, 42f, 30f, 1.1f, 60f, 3.4f);
                    d.weakPointAlwaysExposed = false;
                    d.boss = true;
                    break;
                case EnemyKind.Specter:
                case EnemyKind.PolymorphicClone:
                case EnemyKind.Zombies:
                    maxHealth = 80f;
                    d.behaviorStyle = EnemyBehaviorStyle.RealWorldPossessor;
                    Ranges(d, 24f, 8f, 3.6f, 10f, 1.2f);
                    d.contactDamage = 18f;
                    break;
                case EnemyKind.XanaWilliam:
                    maxHealth = 280f;
                    d.behaviorStyle = EnemyBehaviorStyle.Duelist;
                    Ranges(d, 34f, 18f, 4.2f, 28f, 1f);
                    d.contactDamage = 45f;
                    d.boss = true;
                    break;
                case EnemyKind.ExplodingRoachster:
                    maxHealth = 35f;
                    d.behaviorStyle = EnemyBehaviorStyle.Exploder;
                    Ranges(d, 18f, 4f, 5.5f, 34f, 1f);
                    d.contactDamage = 34f;
                    break;
            }

            d.maxHealth = maxHealth;
            d.weakPointInstantKill = kind == EnemyKind.ExplodingRoachster;
            d.knockbackScale = kind == EnemyKind.Kolossus || kind == EnemyKind.Guardian ? 0f : kind == EnemyKind.Tarantula ? 0.5f : 1f;
            d.staggerMultiplier = d.boss ? 0.3f : 1f;
        }

        private static void Ranges(EnemyDefinition d, float detection, float attackRange, float speed, float damage, float cooldown)
        {
            d.detectionRange = detection;
            d.attackRange = attackRange;
            d.moveSpeed = speed;
            d.projectileDamage = damage;
            d.fireCooldown = cooldown;
        }
    }
}
