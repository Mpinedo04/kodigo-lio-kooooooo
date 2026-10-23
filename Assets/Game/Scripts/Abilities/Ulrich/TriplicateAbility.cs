using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Ulrich's Triplicate (F5.11): two clones with a simple AI that draw enemy fire (power = target priority),
    // hit softly and vanish at the first hit.
    public sealed class TriplicateAbility : Ability
    {
        private const float SideOffset = 1.6f;

        private readonly List<UlrichClone> clones = new List<UlrichClone>();
        private float endsAt;

        public TriplicateAbility(AbilityDefinition definition) : base(definition)
        {
        }

        public int CloneCount
        {
            get
            {
                Prune();
                return clones.Count;
            }
        }

        public override string Status => IsActive ? "Clones: " + CloneCount : null;

        protected override void Activate()
        {
            VanishAll();
            int count = Mathf.Max(1, Definition.count);
            for (int i = 0; i < count; i++)
            {
                float side = (i % 2 == 0 ? -1f : 1f) * SideOffset * (1 + i / 2);
                Vector3 position = Transform.position + Transform.right * side;
                clones.Add(UlrichClone.Spawn(Owner, position, Transform.rotation, Definition.abilityHealth, Definition.damage, Definition.speed, Definition.range, side, Definition.power));
            }

            endsAt = Time.time + Definition.duration;
            Controller.Notify("Triplicado!");
        }

        public override void Tick(float dt)
        {
            if (CloneCount == 0 || Time.time >= endsAt)
            {
                End();
            }
        }

        // Triangular: the clones take two corners around the target.
        public void Surround(Vector3 first, Vector3 second, Transform target)
        {
            Prune();
            if (clones.Count > 0)
            {
                clones[0].Teleport(first, target);
            }

            if (clones.Count > 1)
            {
                clones[1].Teleport(second, target);
            }
        }

        protected override void OnEnd()
        {
            VanishAll();
        }

        private void VanishAll()
        {
            for (int i = 0; i < clones.Count; i++)
            {
                if (clones[i] != null)
                {
                    clones[i].Vanish();
                }
            }

            clones.Clear();
        }

        private void Prune()
        {
            clones.RemoveAll(clone => clone == null || clone.IsGone);
        }
    }
}
