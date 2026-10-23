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

        private static readonly Color VirtualizationColor = new Color(0.3f, 0.9f, 1f);

        private int activeIndex;
        private int handledDevirtualizedIndex = -1;
        private bool controlSuspended;

        public PlayerController ActiveCharacter => characters.Count == 0 ? null : characters[activeIndex];
        public IReadOnlyList<PlayerController> Characters => characters;
        // True while a cutscene (tower interior) has taken control away: everyone fights with AI.
        public bool IsControlSuspended => controlSuspended;

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

            SpreadOut();
            Activate(0);
            for (int i = 0; i < characters.Count; i++)
            {
                VirtualizationEffect.Play(characters[i] != null ? characters[i].transform : null, VirtualizationColor);
            }
        }

        // The level builders drop everyone on one spawn point; companions need room to walk (F5.28).
        private void SpreadOut()
        {
            if (characters.Count == 0 || characters[0] == null)
            {
                return;
            }

            Transform leader = characters[0].transform;
            for (int i = 1; i < characters.Count; i++)
            {
                if (characters[i] == null || (characters[i].transform.position - leader.position).sqrMagnitude > 1f)
                {
                    continue;
                }

                float side = i % 2 == 1 ? -1f : 1f;
                Vector3 spot = leader.position + leader.right * side * 1.6f * ((i + 1) / 2) - leader.forward * 0.8f;
                Teleport(characters[i], spot);
            }
        }

        private static void Teleport(PlayerController character, Vector3 position)
        {
            CharacterController body = character.GetComponent<CharacterController>();
            bool wasEnabled = body != null && body.enabled;
            if (wasEnabled)
            {
                body.enabled = false;
            }

            character.transform.position = position;
            if (wasEnabled)
            {
                body.enabled = true;
            }
        }

        private void Update()
        {
            if (controlSuspended)
            {
                return;
            }

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

            // Everyone stays where they are: the camera jumps to the new warrior and the others carry on with AI.
            activeIndex = index;

            for (int i = 0; i < characters.Count; i++)
            {
                if (IsAlive(i))
                {
                    characters[i].SetControlActive(i == activeIndex);
                }
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

        public void SuspendControl()
        {
            if (controlSuspended)
            {
                return;
            }

            controlSuspended = true;
            for (int i = 0; i < characters.Count; i++)
            {
                if (IsAlive(i))
                {
                    characters[i].SetControlActive(false);
                }
            }
        }

        public void ResumeControl()
        {
            if (!controlSuspended)
            {
                return;
            }

            controlSuspended = false;
            if (characters.Count > 0 && IsAlive(activeIndex))
            {
                Activate(activeIndex);
            }
            else
            {
                ActivateNextAlive(1);
            }
        }

        // Emergency Return to the Past (F7.11): everyone back in Lyoko, full health, at the checkpoint.
        public void ReviveAllAt(Vector3 position, Quaternion facing)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] == null)
                {
                    continue;
                }

                characters[i].gameObject.SetActive(true);
                Health health = characters[i].GetComponent<Health>();
                if (health != null)
                {
                    health.ResetHealth();
                }

                TeamTeleport.Move(characters[i], TeamTeleport.Spot(position, facing, i));
                characters[i].transform.rotation = facing;
                VirtualizationEffect.Play(characters[i].transform, VirtualizationColor);
            }

            controlSuspended = false;
            handledDevirtualizedIndex = -1;
            if (characters.Count > 0)
            {
                Activate(IsAlive(activeIndex) ? activeIndex : 0);
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
