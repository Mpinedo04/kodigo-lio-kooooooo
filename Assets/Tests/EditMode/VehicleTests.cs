using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CodeLyokoFanGame.Tests
{
    public sealed class VehicleTests
    {
        private readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                Object.DestroyImmediate(created[i]);
            }

            created.Clear();
        }

        // ---- Steering and speed ---------------------------------------------------------

        [Test]
        public void SteerYaw_TurnsAtMostTheAllowedDegrees()
        {
            float yaw = VehicleMath.SteerYaw(0f, Vector3.right, 30f);

            Assert.AreEqual(30f, yaw, 0.001f);
        }

        [Test]
        public void SteerYaw_ReachesTheDirectionWhenClose()
        {
            float yaw = VehicleMath.SteerYaw(80f, Vector3.right, 30f);

            Assert.AreEqual(90f, yaw, 0.001f);
        }

        [Test]
        public void SteerYaw_KeepsHeadingWithoutInput()
        {
            Assert.AreEqual(45f, VehicleMath.SteerYaw(45f, Vector3.zero, 30f), 0.001f);
        }

        [Test]
        public void TurnRate_IsWiderAtTopSpeed()
        {
            Assert.AreEqual(150f, VehicleMath.TurnRate(150f, 0.5f, 0f), 0.001f);
            Assert.AreEqual(75f, VehicleMath.TurnRate(150f, 0.5f, 1f), 0.001f);
        }

        [Test]
        public void TargetSpeed_BoostRaisesTopSpeedAndSharpTurnsSlowDown()
        {
            Assert.AreEqual(20f, VehicleMath.TargetSpeed(20f, 1.5f, false, 1f, 1f, 0.5f), 0.001f);
            Assert.AreEqual(30f, VehicleMath.TargetSpeed(20f, 1.5f, true, 1f, 1f, 0.5f), 0.001f);
            Assert.AreEqual(10f, VehicleMath.TargetSpeed(20f, 1.5f, false, 1f, -1f, 0.5f), 0.001f);
            Assert.AreEqual(0f, VehicleMath.TargetSpeed(20f, 1.5f, true, 0f, 1f, 0.5f), 0.001f);
        }

        [Test]
        public void ApproachSpeed_AcceleratesAndCoastsAtTheirOwnRates()
        {
            Assert.AreEqual(2f, VehicleMath.ApproachSpeed(0f, 20f, 20f, 5f, 0.1f), 0.001f);
            Assert.AreEqual(9.5f, VehicleMath.ApproachSpeed(10f, 0f, 20f, 5f, 0.1f), 0.001f);
            Assert.AreEqual(20f, VehicleMath.ApproachSpeed(19.9f, 20f, 20f, 5f, 0.1f), 0.001f);
        }

        // ---- Ram, drift, flight ---------------------------------------------------------

        [Test]
        public void RamDamage_NeedsMinimumSpeedAndGrowsWithSpeed()
        {
            Assert.AreEqual(0f, VehicleMath.RamDamage(40f, 10f, 12f, 30f));
            Assert.AreEqual(24f, VehicleMath.RamDamage(40f, 12f, 12f, 30f), 0.001f);
            Assert.AreEqual(40f, VehicleMath.RamDamage(40f, 30f, 12f, 30f), 0.001f);
            Assert.AreEqual(0f, VehicleMath.RamDamage(0f, 30f, 12f, 30f));
        }

        [Test]
        public void DriftBoost_ShortDriftsGiveNothingAndLongOnesCapAtOne()
        {
            Assert.AreEqual(0f, VehicleMath.DriftBoostStrength(0.2f, 1.2f));
            Assert.AreEqual(0.5f, VehicleMath.DriftBoostStrength(0.6f, 1.2f), 0.001f);
            Assert.AreEqual(1f, VehicleMath.DriftBoostStrength(5f, 1.2f), 0.001f);
        }

        [Test]
        public void ClimbFromPitch_HasADeadZoneAndClamps()
        {
            Assert.AreEqual(0f, VehicleMath.ClimbFromPitch(-9f, -11f, 4f, 10f));
            Assert.AreEqual(0.5f, VehicleMath.ClimbFromPitch(-2f, -11f, 4f, 10f), 0.001f);
            Assert.AreEqual(-1f, VehicleMath.ClimbFromPitch(-45f, -11f, 4f, 10f), 0.001f);
        }

        [Test]
        public void PickNearest_IgnoresTargetsOutOfRange()
        {
            List<Vector3> positions = new List<Vector3> { new Vector3(30f, 0f, 0f), new Vector3(0f, 0f, 8f), new Vector3(0f, 0f, 5f) };

            Assert.AreEqual(2, VehicleMath.PickNearest(Vector3.zero, positions, 24f));
            Assert.AreEqual(-1, VehicleMath.PickNearest(Vector3.zero, positions, 4f));
        }

        // ---- Tricks ---------------------------------------------------------------------

        [Test]
        public void Tricks_LandedTricksChainIntoACombo()
        {
            TrickTracker tricks = new TrickTracker();

            Assert.IsTrue(tricks.TryStart(0f, 0.5f, 0.1f, 0));
            Assert.AreEqual(0f, tricks.Tick(0.3f, 1.5f, 0.5f, 3f));
            Assert.AreEqual(1f, tricks.Tick(0.5f, 1.5f, 0.5f, 3f), 0.001f);
            Assert.AreEqual(1, tricks.Combo);

            Assert.IsTrue(tricks.TryStart(0.7f, 0.5f, 0.1f, 3));
            Assert.AreEqual(1.5f, tricks.Tick(1.2f, 1.5f, 0.5f, 3f), 0.001f);
            Assert.AreEqual(2, tricks.Combo);
        }

        [Test]
        public void Tricks_CannotOverlapAndRespectTheCooldown()
        {
            TrickTracker tricks = new TrickTracker();
            tricks.TryStart(0f, 0.5f, 0.2f, 0);

            Assert.IsFalse(tricks.TryStart(0.2f, 0.5f, 0.2f, 1));
            tricks.Tick(0.5f, 1.5f, 0.5f, 3f);
            Assert.IsFalse(tricks.TryStart(0.6f, 0.5f, 0.2f, 1));
            Assert.IsTrue(tricks.TryStart(0.7f, 0.5f, 0.2f, 1));
        }

        [Test]
        public void Tricks_ComboEndsAfterTheWindowOrWhenCancelled()
        {
            TrickTracker tricks = new TrickTracker();
            tricks.TryStart(0f, 0.5f, 0f, 0);
            tricks.Tick(0.5f, 1f, 0.5f, 3f);

            tricks.Tick(2f, 1f, 0.5f, 3f);
            Assert.AreEqual(0, tricks.Combo);

            tricks.TryStart(3f, 0.5f, 0f, 0);
            tricks.Cancel();
            Assert.IsFalse(tricks.IsActive);
            Assert.AreEqual(0f, tricks.Tick(4f, 1f, 0.5f, 3f));
        }

        // ---- Hits while mounted ---------------------------------------------------------

        [Test]
        public void MountedRider_HitsLandOnTheVehicleUntilRestored()
        {
            GameObject rider = Track(new GameObject("Rider"));
            CapsuleCollider body = rider.AddComponent<CapsuleCollider>();
            Health riderHealth = rider.AddComponent<Health>();
            riderHealth.SetDestroyOnDeath(false);
            riderHealth.SetMaxHealth(100f);
            riderHealth.SetTeam(Team.Player);

            GameObject vehicle = Track(new GameObject("Vehicle"));
            Health vehicleHealth = vehicle.AddComponent<Health>();
            vehicleHealth.SetDestroyOnDeath(false);
            vehicleHealth.SetMaxHealth(150f);
            vehicleHealth.SetTeam(Team.Player);

            DamageInfo laser = new DamageInfo { amount = 20f, type = DamageType.Laser, sourceTeam = Team.Enemy };
            RiderHitRedirect redirect = new RiderHitRedirect(rider);

            redirect.Redirect(vehicleHealth);
            HitResolver.ApplyToCollider(body, laser, Team.Enemy);
            Assert.AreEqual(100f, riderHealth.Current);
            Assert.AreEqual(130f, vehicleHealth.Current);

            redirect.Restore();
            HitResolver.ApplyToCollider(body, laser, Team.Enemy);
            Assert.AreEqual(80f, riderHealth.Current);
            Assert.AreEqual(130f, vehicleHealth.Current);
        }

        [Test]
        public void MountedRider_OwnTeamShotsNeverHurtTheVehicle()
        {
            GameObject rider = Track(new GameObject("Rider"));
            CapsuleCollider body = rider.AddComponent<CapsuleCollider>();
            rider.AddComponent<Health>().SetTeam(Team.Player);

            GameObject vehicle = Track(new GameObject("Vehicle"));
            Health vehicleHealth = vehicle.AddComponent<Health>();
            vehicleHealth.SetDestroyOnDeath(false);
            vehicleHealth.SetMaxHealth(150f);
            vehicleHealth.SetTeam(Team.Player);

            new RiderHitRedirect(rider).Redirect(vehicleHealth);
            Health hit = HitResolver.ApplyToCollider(body, new DamageInfo { amount = 20f, sourceTeam = Team.Player }, Team.Player);

            Assert.IsNull(hit);
            Assert.AreEqual(150f, vehicleHealth.Current);
        }

        private GameObject Track(GameObject gameObject)
        {
            created.Add(gameObject);
            return gameObject;
        }
    }
}
