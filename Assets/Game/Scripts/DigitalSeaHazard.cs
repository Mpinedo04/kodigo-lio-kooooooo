using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Falling into the Digital Sea devirtualizes anything at once (canon).
    // Test scenes can respawn warriors instead so practice is not interrupted.
    public sealed class DigitalSeaHazard : MonoBehaviour
    {
        [SerializeField] private float killY = -12f;
        [SerializeField] private bool respawnWarriorsInstead;
        [SerializeField] private Transform respawnPoint;

        private readonly List<Health> fallen = new List<Health>();

        public void Configure(bool respawnInstead, Transform respawn)
        {
            respawnWarriorsInstead = respawnInstead;
            respawnPoint = respawn;
        }

        private void Update()
        {
            fallen.Clear();
            IReadOnlyList<Health> all = Health.All;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] != null && !all[i].IsDead && all[i].transform.position.y <= killY)
                {
                    fallen.Add(all[i]);
                }
            }

            for (int i = 0; i < fallen.Count; i++)
            {
                if (respawnWarriorsInstead && fallen[i].TryGetComponent(out PlayerController _))
                {
                    Respawn(fallen[i].gameObject);
                }
                else
                {
                    fallen[i].Kill(DamageType.Fall);
                }
            }
        }

        private void Respawn(GameObject warrior)
        {
            CharacterController controller = warrior.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Vector3 fallback = respawnPoint != null ? respawnPoint.position : Vector3.up * 3f;
            warrior.transform.position = CheckpointRegistry.GetRespawnPosition(fallback);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
    }
}
