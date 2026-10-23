using UnityEngine;

namespace CodeLyokoFanGame
{
    // F6.8 Kankrelat: comes in groups that spread on a ring around their target and shoot weak lasers.
    // One hit on the eye devirtualizes it (EnemyDefinition.weakPointInstantKill).
    public sealed class KankrelatBehaviour : EnemyBehaviour
    {
        private float jitterSeed;

        protected override void OnBind()
        {
            jitterSeed = Random.Range(0f, 100f);
        }

        public override Vector3 DesiredPosition(Health target)
        {
            CombatDirector director = Brain.Director;
            if (director == null)
            {
                return base.DesiredPosition(target);
            }

            int slot = director.RingSlot(Host, target, out int count, out Vector3 anchor);
            if (count <= 1 || anchor.sqrMagnitude < 0.01f)
            {
                return base.DesiredPosition(target);
            }

            float baseAngle = Mathf.Atan2(anchor.x, anchor.z);
            float wobble = Mathf.Sin(Brain.Time * 0.7f + jitterSeed) * 0.25f;
            float angle = baseAngle + slot * Mathf.PI * 2f / count + wobble;
            float radius = Definition.preferredDistance + Mathf.Sin(Brain.Time * 1.3f + jitterSeed) * 0.8f;
            return target.transform.position + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
        }

        public override void Tick(float deltaTime)
        {
            // Scuttling: the body bobs while it moves.
            Transform body = Host.BodyVisual;
            if (body != null && Motor.FlatSpeed > 0.3f)
            {
                body.localRotation = Quaternion.Euler(Mathf.Sin(Brain.Time * 22f) * 4f, 0f, Mathf.Cos(Brain.Time * 22f) * 4f);
            }
        }
    }
}
