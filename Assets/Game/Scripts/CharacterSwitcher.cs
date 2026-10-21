using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class CharacterSwitcher : MonoBehaviour
    {
        [SerializeField] private List<PlayerController> characters = new List<PlayerController>();
        [SerializeField] private ThirdPersonCamera thirdPersonCamera;
        [SerializeField] private LockOnSystem lockOnSystem;

        private int activeIndex;
        private int handledDevirtualizedIndex = -1;

        public PlayerController ActiveCharacter => characters.Count == 0 ? null : characters[activeIndex];

        public bool HasAliveCharacter
        {
            get
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    if (IsAlive(i))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Register(PlayerController controller)
        {
            if (!characters.Contains(controller))
            {
                characters.Add(controller);
            }
        }

        private void Start()
        {
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].Initialize(Camera.main != null ? Camera.main.transform : null, lockOnSystem);
            }

            Activate(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Activate(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Activate(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Activate(2);

            // A devirtualized warrior hands control to the next one still in Lyoko.
            if (characters.Count > 0 && activeIndex != handledDevirtualizedIndex && !IsAlive(activeIndex))
            {
                handledDevirtualizedIndex = activeIndex;
                Debug.Log("[Lyoko] " + (ActiveCharacter != null ? ActiveCharacter.name : "Warrior") + " devirtualized.");
                ActivateNextAlive();
            }
        }

        public void Activate(int index)
        {
            if (index < 0 || index >= characters.Count || !IsAlive(index))
            {
                return;
            }

            Vector3 handoffPosition = ActiveCharacter != null ? ActiveCharacter.transform.position : characters[index].transform.position;
            Quaternion handoffRotation = ActiveCharacter != null ? ActiveCharacter.transform.rotation : characters[index].transform.rotation;

            activeIndex = index;

            for (int i = 0; i < characters.Count; i++)
            {
                if (!IsAlive(i))
                {
                    continue;
                }

                characters[i].transform.position = handoffPosition;
                characters[i].transform.rotation = handoffRotation;
                characters[i].SetControlActive(i == activeIndex);
            }

            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.SetTarget(characters[activeIndex].transform);
            }

            if (lockOnSystem != null)
            {
                lockOnSystem.SetOwner(characters[activeIndex].transform);
            }
        }

        private void ActivateNextAlive()
        {
            for (int offset = 1; offset <= characters.Count; offset++)
            {
                int index = (activeIndex + offset) % characters.Count;
                if (IsAlive(index))
                {
                    Activate(index);
                    return;
                }
            }
        }

        private bool IsAlive(int index)
        {
            PlayerController character = characters[index];
            if (character == null)
            {
                return false;
            }

            Health health = character.GetComponent<Health>();
            return health == null || !health.IsDead;
        }
    }
}
