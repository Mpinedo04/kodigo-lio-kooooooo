using UnityEngine;

namespace CodeLyokoFanGame
{
    // Respawn point. Activated when a warrior walks over it or when a mission step starts (MissionStep.checkpointId).
    public sealed class Checkpoint : MonoBehaviour
    {
        [SerializeField] private string id;
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float activationRadius = 4f;

        public string Id => id;
        public Vector3 RespawnPosition => respawnPoint != null ? respawnPoint.position : transform.position + Vector3.up * 2f;
        public bool IsActive { get; private set; }

        public void SetId(string newId)
        {
            id = newId;
        }

        public void Activate()
        {
            IsActive = true;
            CheckpointRegistry.SetActiveCheckpoint(this);
        }

        private void Update()
        {
            if (IsActive)
            {
                return;
            }

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (!players[i].activeInHierarchy)
                {
                    continue;
                }

                if (Vector3.Distance(players[i].transform.position, transform.position) <= activationRadius)
                {
                    Activate();
                    break;
                }
            }
        }
    }

    public static class CheckpointRegistry
    {
        public static Checkpoint ActiveCheckpoint { get; private set; }

        public static void SetActiveCheckpoint(Checkpoint checkpoint)
        {
            ActiveCheckpoint = checkpoint;
        }

        public static Vector3 GetRespawnPosition(Vector3 fallback)
        {
            return ActiveCheckpoint != null ? ActiveCheckpoint.RespawnPosition : fallback;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            ActiveCheckpoint = null;
        }
    }
}
