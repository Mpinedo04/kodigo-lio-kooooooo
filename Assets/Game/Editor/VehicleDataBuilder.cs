using System.IO;
using CodeLyokoFanGame.Data;
using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    // Handling values of the playable vehicles (F08). Tune here and run Rebuild Everything:
    // the generators are the source of truth for VehicleDefinition assets.
    public static class VehicleDataBuilder
    {
        private const string Folder = "Assets/Game/Data/Vehicles";

        // Loads (or creates) the definition asset of a vehicle and applies its handling.
        public static VehicleDefinition Ensure(VehicleStyle style, string assetName)
        {
            string path = Folder + "/" + assetName + ".asset";
            VehicleDefinition definition = AssetDatabase.LoadAssetAtPath<VehicleDefinition>(path);
            if (definition == null)
            {
                Directory.CreateDirectory(Folder);
                definition = ScriptableObject.CreateInstance<VehicleDefinition>();
                definition.style = style;
                definition.displayName = assetName;
                AssetDatabase.CreateAsset(definition, path);
            }

            ApplyHandling(definition);
            return definition;
        }

        public static void ApplyHandling(VehicleDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            switch (definition.style)
            {
                case VehicleStyle.Overboard:
                    Overboard(definition);
                    break;
                case VehicleStyle.Overbike:
                    Overbike(definition);
                    break;
                case VehicleStyle.Overwing:
                    Overwing(definition);
                    break;
                default:
                    return;
            }

            EditorUtility.SetDirty(definition);
        }

        // Odd: agile free flight, tight turns, tricks that fill the special meter, shoots while riding.
        private static void Overboard(VehicleDefinition d)
        {
            d.defaultRider = LyokoCharacter.Odd;
            d.cruiseSpeed = 16f;
            d.boostMultiplier = 1.7f;
            d.acceleration = 24f;
            d.drag = 10f;
            d.cornerSpeedFactor = 0.55f;
            d.turnRate = 210f;
            d.highSpeedTurnFactor = 0.7f;
            d.grip = 12f;
            d.bankAngle = 30f;
            d.canFly = true;
            d.verticalSpeed = 9f;
            d.verticalAcceleration = 30f;
            d.pitchSteering = 0.8f;
            d.neutralCameraPitch = -11f;
            d.pitchDeadZone = 4f;
            d.pitchRange = 10f;
            d.maxAltitude = 45f;
            d.canDrift = false;
            d.ramDamage = 0f;
            d.canTrick = true;
            d.tricksOnlyAirborne = false;
            d.trickDuration = 0.55f;
            d.trickCooldown = 0.1f;
            d.trickMeterGain = 8f;
            d.trickComboWindow = 1.6f;
            d.trickComboBonus = 0.5f;
            d.maxTrickMultiplier = 3f;
            d.maxHealth = 100f;
            d.hitInvulnerability = 0.25f;
            d.respawnCooldown = 6f;
            d.seatHeight = 0.5f;
            d.riderCanShoot = true;
            d.riderCanUseSpecial = true;
            d.hoverBob = 0.08f;
            d.hoverRate = 5f;
            d.canCarryPassenger = false;
        }

        // Ulrich: glued to the ground, the fastest, turbo jump, drift with mini-boost and ramming damage.
        private static void Overbike(VehicleDefinition d)
        {
            d.defaultRider = LyokoCharacter.Ulrich;
            d.cruiseSpeed = 20f;
            d.boostMultiplier = 1.65f;
            d.acceleration = 22f;
            d.drag = 8f;
            d.cornerSpeedFactor = 0.5f;
            d.turnRate = 150f;
            d.highSpeedTurnFactor = 0.5f;
            d.grip = 9f;
            d.bankAngle = 28f;
            d.canFly = false;
            d.pitchSteering = 0f;
            d.gravity = 32f;
            d.groundSnapDistance = 1.2f;
            d.jumpForce = 10f;
            d.boostJumpMultiplier = 1.4f;
            d.canDrift = true;
            d.driftTurnMultiplier = 1.7f;
            d.driftGrip = 1.8f;
            d.driftMinSpeed = 8f;
            d.driftFullChargeTime = 1.2f;
            d.driftBoostSpeed = 8f;
            d.driftBoostDuration = 0.9f;
            d.ramDamage = 45f;
            d.ramMinSpeed = 12f;
            d.ramRadius = 1.3f;
            d.ramKnockback = 9f;
            d.ramRehitDelay = 0.6f;
            d.ramSpeedKept = 0.7f;
            d.canTrick = true;
            d.tricksOnlyAirborne = true;
            d.trickDuration = 0.5f;
            d.trickMeterGain = 10f;
            d.maxHealth = 150f;
            d.hitInvulnerability = 0.25f;
            d.respawnCooldown = 6f;
            d.seatHeight = 0.95f;
            d.riderCanShoot = true;
            d.riderCanUseSpecial = true;
            d.hoverBob = 0f;
            d.canCarryPassenger = false;
        }

        // Yumi: steady, floaty flight with a passenger seat; Aelita shoots from the back.
        private static void Overwing(VehicleDefinition d)
        {
            d.defaultRider = LyokoCharacter.Yumi;
            d.cruiseSpeed = 14f;
            d.boostMultiplier = 1.5f;
            d.acceleration = 14f;
            d.drag = 6f;
            d.cornerSpeedFactor = 0.6f;
            d.turnRate = 110f;
            d.highSpeedTurnFactor = 0.75f;
            d.grip = 4f;
            d.bankAngle = 18f;
            d.canFly = true;
            d.verticalSpeed = 6f;
            d.verticalAcceleration = 14f;
            d.pitchSteering = 0f;
            d.maxAltitude = 40f;
            d.canDrift = false;
            d.ramDamage = 0f;
            d.canTrick = false;
            d.maxHealth = 180f;
            d.hitInvulnerability = 0.3f;
            d.respawnCooldown = 6f;
            d.seatHeight = 0.7f;
            d.riderCanShoot = true;
            d.riderCanUseSpecial = true;
            d.hoverBob = 0.06f;
            d.hoverRate = 3.5f;
            d.canCarryPassenger = true;
            d.passengerSeatOffset = new Vector3(0f, -0.1f, -0.85f);
            d.passengerBoardRadius = 7f;
            d.passengerFireInterval = 1.1f;
            d.passengerFireRange = 24f;
            d.passengerDamage = 10f;
        }
    }
}
