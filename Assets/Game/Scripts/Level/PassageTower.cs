using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Neutral tower used as a passage (F7.8): the controlled warrior walks in and the whole team comes out
    // of the linked tower. With a sector scene set, it loads that sector instead.
    public sealed class PassageTower : MonoBehaviour
    {
        private static float lastTravelTime = float.NegativeInfinity;

        [SerializeField] private PassageTower exit;
        [Tooltip("Optional: level to load instead of using the linked tower.")]
        [SerializeField] private string sectorScene;
        [SerializeField] private float entryRadius = 2.2f;
        [SerializeField] private float exitDistance = 3.5f;
        [SerializeField] private float cooldown = 2f;

        public Vector3 ExitPoint => transform.position + transform.forward * exitDistance;

        public void Link(PassageTower other)
        {
            exit = other;
        }

        private void Update()
        {
            if (Time.time < lastTravelTime + cooldown)
            {
                return;
            }

            IReadOnlyList<PlayerController> warriors = PlayerController.All;
            for (int i = 0; i < warriors.Count; i++)
            {
                if (!warriors[i].IsControlled)
                {
                    continue;
                }

                Vector3 flat = Vector3.ProjectOnPlane(warriors[i].transform.position - transform.position, Vector3.up);
                if (flat.magnitude <= entryRadius)
                {
                    Travel();
                }

                return;
            }
        }

        private void Travel()
        {
            lastTravelTime = Time.time;
            if (!string.IsNullOrEmpty(sectorScene) && SceneLoader.Instance != null)
            {
                Debug.Log("[Lyoko] Torre de paso hacia " + sectorScene);
                SceneLoader.Instance.LoadLevel(sectorScene);
                return;
            }

            if (exit == null)
            {
                return;
            }

            Debug.Log("[Lyoko] Torre de paso: " + name + " -> " + exit.name);
            TeamTeleport.Squad(exit.ExitPoint, Quaternion.LookRotation(exit.transform.forward));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = TowerVisual.ColorOf(TowerState.Neutral);
            Gizmos.DrawWireSphere(transform.position, entryRadius);
            if (exit != null)
            {
                Gizmos.DrawLine(transform.position, exit.transform.position);
            }
        }
    }
}
