using UnityEngine;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Builds placeholder enemies at runtime (waves, debug spawning) until real prefabs exist.
    public static class EnemyFactory
    {
        public static EnemyAI CreatePlaceholder(EnemyDefinition definition, EnemyKind fallbackKind, Vector3 position, Projectile projectile)
        {
            GameObject enemyObject = new GameObject(definition != null ? definition.displayName : fallbackKind.ToString());
            enemyObject.tag = "Enemy";
            enemyObject.transform.position = position;
            enemyObject.AddComponent<Health>();
            EnemyAI enemy = enemyObject.AddComponent<EnemyAI>();

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "RuntimeWavePlaceholder";
            body.transform.SetParent(enemyObject.transform);
            body.transform.localPosition = Vector3.up;
            body.transform.localScale = Vector3.one;
            Object.Destroy(body.GetComponent<Collider>());

            GameObject hitCollider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hitCollider.name = "HitCollider";
            hitCollider.tag = "Enemy";
            hitCollider.transform.SetParent(enemyObject.transform);
            hitCollider.transform.localPosition = Vector3.up;
            hitCollider.GetComponent<Renderer>().enabled = false;

            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "XanaEyeWeakPoint";
            eye.tag = "Enemy";
            eye.transform.SetParent(enemyObject.transform);
            eye.transform.localPosition = new Vector3(0f, 1.25f, 0.6f);
            eye.transform.localScale = Vector3.one * 0.32f;
            eye.GetComponent<Collider>().isTrigger = true;
            eye.AddComponent<WeakPoint>();

            Transform firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(enemyObject.transform, false);
            firePoint.localPosition = new Vector3(0f, 1.25f, 0.9f);
            enemy.SetAnchors(firePoint, eye.transform);

            GameLayers.SetRecursively(enemyObject, GameLayers.Enemy);

            if (definition != null)
            {
                enemy.Configure(definition, projectile);
            }
            else
            {
                enemy.Configure(fallbackKind, projectile);
            }

            return enemy;
        }
    }
}
