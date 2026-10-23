using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeLyokoFanGame
{
    // Single entry point for player input. Gameplay scripts poll these properties
    // instead of touching devices, so bindings can change without code changes.
    [CreateAssetMenu(menuName = "Code Lyoko/Input Reader", fileName = "InputReader")]
    public sealed class InputReader : ScriptableObject
    {
        [SerializeField] private InputActionAsset actions;
        [SerializeField] private float stickLookScale = 55f;

        private InputActionMap lyokoMap;
        private InputActionMap vehicleMap;
        private InputActionMap uiMap;

        private InputAction move;
        private InputAction look;
        private InputAction jump;
        private InputAction sprint;
        private InputAction dodge;
        private InputAction guard;
        private InputAction attack;
        private InputAction special;
        private InputAction ability1;
        private InputAction ability2;
        private InputAction ultimate;
        private InputAction lockOn;
        private InputAction vehicle;
        private InputAction interact;
        private InputAction nextCharacter;
        private InputAction previousCharacter;
        private InputAction orderAelita;
        private InputAction openOperator;
        private InputAction swapFront;
        private readonly InputAction[] switchCharacter = new InputAction[4];

        private InputAction steer;
        private InputAction ascend;
        private InputAction descend;
        private InputAction boost;
        private InputAction fire;
        private InputAction dismount;
        private InputAction trick;
        private InputAction passenger;

        private InputAction pause;
        private InputAction retry;
        private InputAction toggleDebug;

        private bool ready;

        public InputActionAsset Actions => actions;
        public bool VehicleMode { get; private set; }

        // Lyoko (on foot)
        public Vector2 Move => Read(move);
        public Vector2 LookDelta
        {
            get
            {
                Vector2 value = Read(look);
                return LookFromGamepad ? value * stickLookScale * Time.deltaTime : value;
            }
        }
        // Unscaled look value (mouse delta * 0.1 or stick -1..1), e.g. for flick gestures.
        public Vector2 LookRaw => Read(look);
        public bool LookFromGamepad => look != null && look.activeControl != null && look.activeControl.device is Gamepad;
        public bool JumpPressed => Pressed(jump);
        public bool JumpHeld => Held(jump);
        public bool SprintHeld => Held(sprint);
        public bool DodgePressed => Pressed(dodge);
        public bool GuardPressed => Pressed(guard);
        public bool GuardHeld => Held(guard);
        public bool AttackPressed => Pressed(attack);
        public bool AttackHeld => Held(attack);
        public bool SpecialPressed => Pressed(special);
        public bool Ability1Pressed => Pressed(ability1);
        public bool Ability2Pressed => Pressed(ability2);
        public bool UltimatePressed => Pressed(ultimate);
        public bool LockOnPressed => Pressed(lockOn);
        public bool VehiclePressed => Pressed(vehicle);
        public bool InteractPressed => Pressed(interact);
        public bool InteractHeld => Held(interact);
        public bool NextCharacterPressed => Pressed(nextCharacter);
        public bool PreviousCharacterPressed => Pressed(previousCharacter);
        public bool OrderAelitaPressed => Pressed(orderAelita);
        public bool OrderAelitaHeld => Held(orderAelita);
        public bool OpenOperatorPressed => Pressed(openOperator);
        public bool SwapFrontPressed => Pressed(swapFront);

        // Index of the character requested with the number keys this frame, or -1.
        public int SwitchCharacterRequest
        {
            get
            {
                EnsureReady();
                for (int i = 0; i < switchCharacter.Length; i++)
                {
                    if (Pressed(switchCharacter[i]))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        // Vehicle (mounted)
        public Vector2 Steer => Read(steer);
        public bool AscendHeld => Held(ascend);
        public bool AscendPressed => Pressed(ascend);
        public bool DescendHeld => Held(descend);
        public bool BoostHeld => Held(boost);
        public bool FirePressed => Pressed(fire);
        public bool DismountPressed => Pressed(dismount);
        public bool TrickPressed => Pressed(trick);
        public bool PassengerPressed => Pressed(passenger);

        // UI
        public bool PausePressed => Pressed(pause);
        public bool RetryPressed => Pressed(retry);
        public bool ToggleDebugPressed => Pressed(toggleDebug);

        public void SetActions(InputActionAsset asset)
        {
            actions = asset;
            ready = false;
        }

        // The Lyoko map stays on while mounted so camera, lock-on and switching keep working.
        public void SetVehicleMode(bool enabled)
        {
            EnsureReady();
            VehicleMode = enabled;

            if (vehicleMap == null)
            {
                return;
            }

            if (enabled)
            {
                vehicleMap.Enable();
            }
            else
            {
                vehicleMap.Disable();
            }
        }

        private void OnDisable()
        {
            if (actions != null)
            {
                actions.Disable();
            }

            ready = false;
        }

        private void EnsureReady()
        {
            if (ready || actions == null)
            {
                return;
            }

            lyokoMap = actions.FindActionMap("Lyoko");
            vehicleMap = actions.FindActionMap("Vehicle");
            uiMap = actions.FindActionMap("UI");

            move = Find(lyokoMap, "Move");
            look = Find(lyokoMap, "Look");
            jump = Find(lyokoMap, "Jump");
            sprint = Find(lyokoMap, "Sprint");
            dodge = Find(lyokoMap, "Dodge");
            guard = Find(lyokoMap, "Guard");
            attack = Find(lyokoMap, "Attack");
            special = Find(lyokoMap, "Special");
            ability1 = Find(lyokoMap, "Ability1");
            ability2 = Find(lyokoMap, "Ability2");
            ultimate = Find(lyokoMap, "Ultimate");
            lockOn = Find(lyokoMap, "LockOn");
            vehicle = Find(lyokoMap, "Vehicle");
            interact = Find(lyokoMap, "Interact");
            nextCharacter = Find(lyokoMap, "NextCharacter");
            previousCharacter = Find(lyokoMap, "PreviousCharacter");
            orderAelita = Find(lyokoMap, "OrderAelita");
            openOperator = Find(lyokoMap, "OpenOperator");
            swapFront = Find(lyokoMap, "SwapFront");
            for (int i = 0; i < switchCharacter.Length; i++)
            {
                switchCharacter[i] = Find(lyokoMap, "SwitchCharacter" + (i + 1));
            }

            steer = Find(vehicleMap, "Steer");
            ascend = Find(vehicleMap, "Ascend");
            descend = Find(vehicleMap, "Descend");
            boost = Find(vehicleMap, "Boost");
            fire = Find(vehicleMap, "Fire");
            dismount = Find(vehicleMap, "Dismount");
            trick = Find(vehicleMap, "Trick");
            passenger = Find(vehicleMap, "Passenger");

            pause = Find(uiMap, "Pause");
            retry = Find(uiMap, "Retry");
            toggleDebug = Find(uiMap, "ToggleDebug");

            if (Application.isPlaying)
            {
                lyokoMap?.Enable();
                uiMap?.Enable();
                if (VehicleMode)
                {
                    vehicleMap?.Enable();
                }
            }

            ready = true;
        }

        private static InputAction Find(InputActionMap map, string actionName)
        {
            return map != null ? map.FindAction(actionName) : null;
        }

        private Vector2 Read(InputAction action)
        {
            EnsureReady();
            return action != null ? action.ReadValue<Vector2>() : Vector2.zero;
        }

        private bool Pressed(InputAction action)
        {
            EnsureReady();
            return action != null && action.WasPressedThisFrame();
        }

        private bool Held(InputAction action)
        {
            EnsureReady();
            return action != null && action.IsPressed();
        }
    }
}
