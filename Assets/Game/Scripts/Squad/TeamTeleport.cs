using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Moves warriors safely (CharacterController off while teleporting): passage towers, checkpoints, tower interiors.
    public static class TeamTeleport
    {
        public static void Move(Component body, Vector3 position)
        {
            if (body == null)
            {
                return;
            }

            CharacterController controller = body.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }

            body.transform.position = position;
            if (wasEnabled)
            {
                controller.enabled = true;
            }
        }

        // Spot number "index" in a small fan around a point, so nobody lands on top of anyone else.
        public static Vector3 Spot(Vector3 center, Quaternion facing, int index)
        {
            if (index == 0)
            {
                return center;
            }

            float side = index % 2 == 1 ? -1f : 1f;
            return center + facing * new Vector3(side * 1.6f * ((index + 1) / 2), 0f, -1f * ((index + 1) / 2));
        }

        // Every warrior still in Lyoko plus Aelita, to the same place.
        public static void Squad(Vector3 center, Quaternion facing)
        {
            List<PlayerController> warriors = new List<PlayerController>(PlayerController.All);
            int index = 0;
            for (int i = 0; i < warriors.Count; i++)
            {
                Move(warriors[i], Spot(center, facing, index++) + Vector3.up * 0.1f);
                warriors[i].transform.rotation = facing;
            }

            AelitaCompanion aelita = Object.FindObjectOfType<AelitaCompanion>();
            if (aelita != null)
            {
                Move(aelita, Spot(center, facing, index) + Vector3.up * 0.1f);
            }
        }
    }
}
