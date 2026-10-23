using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // One ability of a warrior (F5.2). Each one is a subclass configured by an AbilityDefinition;
    // AbilityController owns them, pays their cost and ticks them every frame.
    public abstract class Ability
    {
        private float readyAt;

        protected Ability(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public AbilityDefinition Definition { get; }
        public bool IsActive { get; private set; }
        public bool IsPassive => Definition.slot == AbilitySlot.Passive;
        public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);
        public float Cooldown01 => Definition.cooldown > 0f ? Mathf.Clamp01(CooldownRemaining / Definition.cooldown) : 0f;

        // Telekinesis takes the attack button while it holds something.
        public virtual bool BlocksAttacks => false;

        // Extra HUD text while it runs (shield points, telekinesis hints...).
        public virtual string Status => null;

        // Why CanUse() said no, shown on the HUD ("no Creativity zone nearby"...).
        public virtual string UnavailableReason => null;

        protected AbilityController Controller { get; private set; }
        protected PlayerController Owner => Controller.Owner;
        protected Transform Transform => Controller.transform;

        internal void Bind(AbilityController controller)
        {
            Controller = controller;
            OnEquip();
        }

        public virtual bool CanUse()
        {
            return !IsActive && CooldownRemaining <= 0f;
        }

        // Called by AbilityController once the cost is paid.
        internal void Begin()
        {
            IsActive = true;
            Activate();
        }

        protected abstract void Activate();

        // Pressing the button again while the ability runs (default: stop it early).
        public virtual void OnPressedAgain()
        {
            End();
        }

        public virtual void Tick(float dt)
        {
        }

        public void End()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            StartCooldown(Definition.cooldown);
            OnEnd();
        }

        protected virtual void OnEnd()
        {
        }

        public virtual bool TryIntercept(DamageInfo info)
        {
            return false;
        }

        public virtual void OnEquip()
        {
        }

        public virtual void OnUnequip()
        {
        }

        protected void StartCooldown(float seconds)
        {
            readyAt = Time.time + Mathf.Max(0f, seconds);
        }
    }
}
