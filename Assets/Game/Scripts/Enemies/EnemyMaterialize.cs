using UnityEngine;

namespace CodeLyokoFanGame
{
    // F06.7: a monster appearing from a wave grows inside a column of X.A.N.A. light.
    // Meanwhile it is invulnerable and its brain is paused.
    public sealed class EnemyMaterialize : MonoBehaviour
    {
        private EnemyAI enemy;
        private Health health;
        private Vector3 targetScale;
        private GameObject column;
        private float duration;
        private float age;
        private bool finished;

        public static EnemyMaterialize Begin(EnemyAI enemy, float duration)
        {
            if (enemy == null || duration <= 0f || !Application.isPlaying)
            {
                return null;
            }

            EnemyMaterialize effect = enemy.gameObject.AddComponent<EnemyMaterialize>();
            effect.enemy = enemy;
            effect.health = enemy.Health;
            effect.duration = duration;
            effect.targetScale = enemy.transform.localScale;
            enemy.transform.localScale = effect.targetScale * 0.05f;
            enemy.SetMaterializing(true);
            if (effect.health != null)
            {
                effect.health.Invulnerable = true;
            }

            effect.column = EnemyVisuals.Shape(PrimitiveType.Cylinder, "MaterializeColumn", null, enemy.transform.position + Vector3.up * 2f,
                new Vector3(1.8f, 2f, 1.8f), new Color(1f, 0.2f, 0.15f));
            return effect;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            float eased = 1f - (1f - t) * (1f - t);
            transform.localScale = targetScale * Mathf.Lerp(0.05f, 1f, eased);

            if (column != null)
            {
                float width = Mathf.Lerp(1.8f, 0f, t);
                column.transform.position = transform.position + Vector3.up * 2f;
                column.transform.localScale = new Vector3(width, 2f + t * 2f, width);
            }

            if (t >= 1f)
            {
                Finish();
            }
        }

        private void OnDestroy()
        {
            Finish();
        }

        private void Finish()
        {
            if (finished)
            {
                return;
            }

            finished = true;
            if (enemy != null)
            {
                transform.localScale = targetScale;
                enemy.SetMaterializing(false);
            }

            if (health != null)
            {
                health.Invulnerable = false;
            }

            if (column != null)
            {
                Destroy(column);
            }

            Destroy(this);
        }
    }
}
