using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class MissionManager : MonoBehaviour
    {
        [SerializeField] private TowerObjective tower;
        [SerializeField] private CharacterSwitcher switcher;

        private GUIStyle boxStyle;
        private GUIStyle titleStyle;

        private void Awake()
        {
            boxStyle = new GUIStyle();
            titleStyle = new GUIStyle();
        }

        private void OnGUI()
        {
            if (boxStyle.normal.background == null)
            {
                Texture2D texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.62f));
                texture.Apply();
                boxStyle.normal.background = texture;
                boxStyle.padding = new RectOffset(14, 14, 10, 10);
                boxStyle.normal.textColor = Color.white;
                boxStyle.fontSize = 16;
                titleStyle.normal.textColor = Color.cyan;
                titleStyle.fontSize = 20;
                titleStyle.fontStyle = FontStyle.Bold;
            }

            GUILayout.BeginArea(new Rect(18, 18, 520, 170), boxStyle);
            GUILayout.Label("CODE LYOKO: Sector Bosque", titleStyle);

            string active = switcher != null && switcher.ActiveCharacter != null ? switcher.ActiveCharacter.Character.ToString() : "N/A";
            GUILayout.Label("Personaje: " + active + "   |   1 Odd  2 Ulrich  3 Yumi  |  V vehiculo  Tab lock-on");

            if (tower != null && tower.IsCleared)
            {
                GUILayout.Label("Torre desactivada. Mision completada. Retorno al pasado listo.");
            }
            else if (tower != null)
            {
                GUILayout.Label("Objetivo: escolta a Aelita hasta la torre y manten E para introducir CODE: LYOKO.");
                GUILayout.Label("Progreso torre: " + Mathf.RoundToInt(tower.Progress01 * 100f) + "%");
            }

            GUILayout.EndArea();
        }
    }
}
