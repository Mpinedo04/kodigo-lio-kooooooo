using System.Collections.Generic;
using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class CharacterSwitcher : MonoBehaviour
    {
        [SerializeField] private List<PlayerController> characters = new List<PlayerController>();
        [SerializeField] private CameraRig cameraRig;
        [SerializeField] private LockOnSystem lockOnSystem;
        [SerializeField] private InputReader input;

        private int activeIndex;
        private int handledDevirtualizedIndex = -1;

        public PlayerController ActiveCharacter => characters.Count == 0 ? null : characters[activeIndex];
        public IReadOnlyList<PlayerController> Characters => characters;

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
                characters[i].Initialize(Camera.main != null ? Camera.main.transform : null, lockOnSystem, input);
            }

            Activate(0);
        }

        private void Update()
        {
            if (input != null)
            {
                int requested = input.SwitchCharacterRequest;
                if (requested >= 0)
                {
                    Activate(requested);
                }

                if (input.NextCharacterPressed)
                {
                    ActivateNextAlive(1);
                }

                if (input.PreviousCharacterPressed)
                {
                    ActivateNextAlive(-1);
                }
            }

            // A devirtualized warrior hands control to the next one still in Lyoko.
            if (characters.Count > 0 && activeIndex != handledDevirtualizedIndex && !IsAlive(activeIndex))
            {
                handledDevirtualizedIndex = activeIndex;
                Debug.Log("[Lyoko] " + (ActiveCharacter != null ? ActiveCharacter.name : "Warrior") + " devirtualized.");
                GameManager.RaiseEvent(GameEventId.PlayerDevirtualized);
                ActivateNextAlive(1);
            }
        }

        public void Activate(int index)
        {
            if (index < 0 || index >= characters.Count || !IsAlive(index))
            {
                return;
            }

            PlayerController outgoing = ActiveCharacter;
            Vector3 handoffPosition = characters[index].transform.position;
            Quaternion handoffRotation = characters[index].transform.rotation;
            if (outgoing != null)
            {
                // A devirtualized warrior may have fallen into the Digital Sea: never hand off there.
                handoffPosition = IsAlive(activeIndex) ? outgoing.transform.position : outgoing.LastGroundedPosition + Vector3.up * 0.2f;
                handoffRotation = outgoing.transform.rotation;
            }

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

            if (cameraRig != null)
            {
                cameraRig.SetTarget(characters[activeIndex]);
            }

            if (lockOnSystem != null)
            {
                lockOnSystem.SetOwner(characters[activeIndex].transform);
            }
        }

        private void ActivateNextAlive(int direction)
        {
            int count = characters.Count;
            for (int offset = 1; offset <= count; offset++)
            {
                int index = ((activeIndex + offset * direction) % count + count) % count;
                if (index != activeIndex && IsAlive(index))
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
