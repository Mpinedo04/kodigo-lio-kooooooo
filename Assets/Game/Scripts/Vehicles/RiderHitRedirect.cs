using UnityEngine;

namespace CodeLyokoFanGame
{
    // While mounted, hits on the rider's body land on the vehicle: a Hurtbox on the rider root points
    // HitResolver at the vehicle's Health, and Restore puts things back as they were.
    public sealed class RiderHitRedirect
    {
        private readonly GameObject rider;
        private Hurtbox hurtbox;
        private bool createdHurtbox;
        private Health previousOwner;
        private float previousMultiplier = 1f;

        public bool Active { get; private set; }

        public RiderHitRedirect(GameObject riderObject)
        {
            rider = riderObject;
        }

        public void Redirect(Health target, float multiplier = 1f)
        {
            if (rider == null || target == null)
            {
                return;
            }

            if (hurtbox == null)
            {
                createdHurtbox = !rider.TryGetComponent(out hurtbox);
                if (createdHurtbox)
                {
                    hurtbox = rider.AddComponent<Hurtbox>();
                }
            }

            if (!Active)
            {
                previousOwner = createdHurtbox ? null : hurtbox.Owner;
                previousMultiplier = createdHurtbox ? 1f : hurtbox.DamageMultiplier;
            }

            hurtbox.Configure(target, multiplier);
            Active = true;
        }

        public void Restore()
        {
            if (!Active || hurtbox == null || rider == null)
            {
                Active = false;
                return;
            }

            Health owner = previousOwner != null ? previousOwner : rider.GetComponent<Health>();
            hurtbox.Configure(owner, previousMultiplier);
            Active = false;
        }
    }
}
