using System.Collections.Generic;
using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Aelita's Creativity (F5.23): a temporary bridge or wall, only at the zones the level marks with CreativityAnchor.
    // With Yumi close by, they come out huge (synergy).
    public sealed class CreativityAbility : Ability
    {
        private readonly CreativityKind kind;

        public CreativityAbility(AbilityDefinition definition, CreativityKind kind) : base(definition)
        {
            this.kind = kind;
        }

        public override string UnavailableReason => kind == CreativityKind.Bridge
            ? "Creatividad: acercate a una zona de puente"
            : "Creatividad: acercate a una zona de muro";

        public override bool CanUse()
        {
            return base.CanUse() && FindAnchor() != null;
        }

        protected override void Activate()
        {
            CreativityAnchor anchor = FindAnchor();
            if (anchor != null)
            {
                TeamSynergy.CreativityBoost(Transform.position, out float size, out float duration);
                anchor.Build(Definition.duration * duration, size);
            }

            End();
        }

        private CreativityAnchor FindAnchor()
        {
            CreativityAnchor best = null;
            float bestDistance = Definition.range * Definition.range;
            IReadOnlyList<CreativityAnchor> anchors = CreativityAnchor.All;
            for (int i = 0; i < anchors.Count; i++)
            {
                if (anchors[i].Kind != kind || anchors[i].InUse)
                {
                    continue;
                }

                float distance = (anchors[i].transform.position - Transform.position).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = anchors[i];
                }
            }

            return best;
        }
    }
}
