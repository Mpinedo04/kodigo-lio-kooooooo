using UnityEngine;

namespace CodeLyokoFanGame
{
    // Orders to Aelita (F7.2). Tap Z / dpad down: next order. Hold it: quick wheel, pick with the move stick or WASD
    // (up Follow, right Wait, down Hide, left Go to the tower) and let go. Ignored while riding (dpad down = passenger).
    public sealed class AelitaOrders : MonoBehaviour
    {
        [SerializeField] private AelitaCompanion aelita;
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private InputReader input;
        [SerializeField] private float holdForWheel = 0.25f;
        [SerializeField] private float confirmationSeconds = 1.5f;

        private float heldTime;
        private bool wheelOpen;
        private AelitaOrder highlighted;
        private float confirmedAt = float.NegativeInfinity;
        private GUIStyle optionStyle;
        private GUIStyle selectedStyle;

        public bool WheelOpen => wheelOpen;

        public static string Label(AelitaOrder order)
        {
            switch (order)
            {
                case AelitaOrder.Wait:
                    return "Esperar aqui";
                case AelitaOrder.Hide:
                    return "Esconderse";
                case AelitaOrder.GoToTower:
                    return "Ir a la torre";
                default:
                    return "Seguirme";
            }
        }

        // Stick direction to order: up Follow, right Wait, down Hide, left Go to the tower.
        public static AelitaOrder Pick(Vector2 direction)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                return direction.x > 0f ? AelitaOrder.Wait : AelitaOrder.GoToTower;
            }

            return direction.y > 0f ? AelitaOrder.Follow : AelitaOrder.Hide;
        }

        public static AelitaOrder Next(AelitaOrder order)
        {
            return (AelitaOrder)(((int)order + 1) % 4);
        }

        public void Configure(AelitaCompanion companion, CharacterSwitcher characterSwitcher, InputReader reader)
        {
            aelita = companion;
            switcher = characterSwitcher;
            input = reader;
        }

        private void Update()
        {
            if (aelita == null || input == null || !aelita.isActiveAndEnabled)
            {
                Close();
                return;
            }

            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            if (player != null && player.IsMounted)
            {
                Close();
                return;
            }

            if (input.OrderAelitaHeld)
            {
                heldTime += Time.unscaledDeltaTime;
                if (!wheelOpen && heldTime >= holdForWheel)
                {
                    wheelOpen = true;
                    highlighted = aelita.Order;
                    PlayerController.MoveInputBlocked = true;
                }

                if (wheelOpen && input.Move.sqrMagnitude > 0.25f)
                {
                    highlighted = Pick(input.Move);
                }

                return;
            }

            if (heldTime > 0f)
            {
                aelita.SetOrder(wheelOpen ? highlighted : Next(aelita.Order));
                confirmedAt = Time.unscaledTime;
                Close();
            }
        }

        private void Close()
        {
            heldTime = 0f;
            if (wheelOpen)
            {
                wheelOpen = false;
                PlayerController.MoveInputBlocked = false;
            }
        }

        private void OnDisable()
        {
            Close();
        }

        private void OnGUI()
        {
            if (aelita == null)
            {
                return;
            }

            EnsureStyles();
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (wheelOpen)
            {
                Option(AelitaOrder.Follow, center + new Vector2(0f, -90f));
                Option(AelitaOrder.Wait, center + new Vector2(150f, 0f));
                Option(AelitaOrder.Hide, center + new Vector2(0f, 90f));
                Option(AelitaOrder.GoToTower, center + new Vector2(-150f, 0f));
                GUI.Label(new Rect(center.x - 120f, center.y - 15f, 240f, 30f), "ORDENES A AELITA", optionStyle);
                return;
            }

            if (Time.unscaledTime - confirmedAt < confirmationSeconds)
            {
                GUI.Label(new Rect(center.x - 200f, Screen.height * 0.22f, 400f, 30f), "Aelita: " + Label(aelita.Order), selectedStyle);
            }
        }

        private void Option(AelitaOrder order, Vector2 position)
        {
            GUI.Label(new Rect(position.x - 100f, position.y - 15f, 200f, 30f), Label(order), order == highlighted ? selectedStyle : optionStyle);
        }

        private void EnsureStyles()
        {
            if (optionStyle != null)
            {
                return;
            }

            optionStyle = new GUIStyle { fontSize = 20, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
            selectedStyle = new GUIStyle(optionStyle) { fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.45f, 0.8f) } };
        }
    }
}
