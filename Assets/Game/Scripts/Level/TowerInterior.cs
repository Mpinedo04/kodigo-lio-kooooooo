using Cinemachine;
using UnityEngine;

namespace CodeLyokoFanGame
{
    // Inside of a tower (F7.7): Aelita floats up to the top platform and types CODE: LYOKO on the panel
    // (hold Interact). Simple playable sequence; Timeline replaces it in F17. Outside, the warriors fight on with AI.
    public sealed class TowerInterior : MonoBehaviour
    {
        public const string Code = "CODE: LYOKO";

        private enum Step
        {
            Idle,
            Floating,
            Typing,
            Done
        }

        [SerializeField] private Transform bottom;
        [SerializeField] private Transform top;
        [SerializeField] private Transform avatar;
        [SerializeField] private CinemachineVirtualCamera interiorCamera;
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private InputReader input;
        [SerializeField] private float floatSeconds = 3f;
        [SerializeField] private float typeSeconds = 3f;
        [SerializeField] private float exitDelay = 1.2f;

        private TowerObjective tower;
        private AelitaCompanion aelita;
        private Step step;
        private float timer;
        private float typed;
        private GUIStyle codeStyle;

        public bool IsRunning => step != Step.Idle;
        public float Progress01 => step == Step.Done ? 1f : step == Step.Typing ? typed : 0f;

        public static string TypedText(float progress01)
        {
            int letters = Mathf.Clamp(Mathf.FloorToInt(progress01 * Code.Length + 0.0001f), 0, Code.Length);
            return Code.Substring(0, letters);
        }

        public void Configure(Transform bottomPoint, Transform topPoint, Transform aelitaAvatar, CinemachineVirtualCamera camera, CharacterSwitcher characterSwitcher, InputReader reader)
        {
            bottom = bottomPoint;
            top = topPoint;
            avatar = aelitaAvatar;
            interiorCamera = camera;
            switcher = characterSwitcher;
            input = reader;
        }

        private void Awake()
        {
            if (avatar != null)
            {
                avatar.gameObject.SetActive(false);
            }

            if (interiorCamera != null)
            {
                interiorCamera.Priority = 0;
            }
        }

        public void Begin(TowerObjective activatedTower, AelitaCompanion companion)
        {
            if (IsRunning)
            {
                return;
            }

            tower = activatedTower;
            aelita = companion;
            step = Step.Floating;
            timer = 0f;
            typed = 0f;

            // Inside the tower Aelita is safe: the monsters lose her until she comes out.
            if (aelita != null)
            {
                if (aelita.IsSeated)
                {
                    aelita.LeaveSeat(aelita.transform.position);
                }

                aelita.gameObject.SetActive(false);
            }

            if (avatar != null)
            {
                avatar.position = bottom != null ? bottom.position : transform.position;
                avatar.gameObject.SetActive(true);
            }

            if (interiorCamera != null)
            {
                interiorCamera.Priority = 100;
            }

            if (switcher != null)
            {
                switcher.SuspendControl();
            }

            Debug.Log("[Tower] Aelita entra en la torre.");
        }

        private void Update()
        {
            if (step == Step.Idle)
            {
                return;
            }

            float dt = Time.deltaTime;
            timer += dt;

            switch (step)
            {
                case Step.Floating:
                    float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / floatSeconds));
                    if (avatar != null && bottom != null && top != null)
                    {
                        avatar.position = Vector3.Lerp(bottom.position, top.position, t);
                    }

                    if (timer >= floatSeconds)
                    {
                        step = Step.Typing;
                        timer = 0f;
                    }

                    break;
                case Step.Typing:
                    // With no input (automatic tests) she types on her own.
                    if (input == null || input.InteractHeld)
                    {
                        typed = Mathf.Clamp01(typed + dt / typeSeconds);
                    }

                    if (typed >= 1f)
                    {
                        step = Step.Done;
                        timer = 0f;
                        Debug.Log("[Tower] " + Code);
                        if (tower != null)
                        {
                            tower.Deactivate();
                        }
                    }

                    break;
                case Step.Done:
                    if (timer >= exitDelay)
                    {
                        Exit();
                    }

                    break;
            }
        }

        private void Exit()
        {
            step = Step.Idle;
            if (avatar != null)
            {
                avatar.gameObject.SetActive(false);
            }

            if (interiorCamera != null)
            {
                interiorCamera.Priority = 0;
            }

            if (aelita != null)
            {
                aelita.gameObject.SetActive(true);
            }

            if (switcher != null)
            {
                switcher.ResumeControl();
            }

            Debug.Log("[Tower] Aelita sale de la torre.");
        }

        private void OnGUI()
        {
            if (step != Step.Typing && step != Step.Done)
            {
                return;
            }

            if (codeStyle == null)
            {
                codeStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 46,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(0.2f, 0.95f, 1f) }
                };
            }

            Rect area = new Rect(0f, Screen.height * 0.35f, Screen.width, 80f);
            GUI.Label(area, TypedText(Progress01) + (step == Step.Typing && Mathf.Repeat(Time.unscaledTime, 0.8f) < 0.4f ? "_" : string.Empty), codeStyle);
            if (step == Step.Typing)
            {
                GUI.Label(new Rect(0f, area.yMax + 10f, Screen.width, 30f), "Manten Interactuar (E / RT) para escribir el codigo", new GUIStyle(codeStyle) { fontSize = 18 });
            }
        }
    }
}
