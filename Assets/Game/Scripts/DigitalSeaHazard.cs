using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class DigitalSeaHazard : MonoBehaviour
    {
        [SerializeField] private float killY = -12f;
        [SerializeField] private Transform respawnPoint;

        private void Update()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < players.Length; i++)
            {
                if (!players[i].activeInHierarchy || players[i].transform.position.y > killY)
                {
                    continue;
                }

                CharacterController controller = players[i].GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                players[i].transform.position = respawnPoint != null ? respawnPoint.position : Vector3.up * 3f;

                if (controller != null)
                {
                    controller.enabled = true;
                }

                Health health = players[i].GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(20f, gameObject);
                }
            }
        }
    }
}
