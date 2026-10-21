using UnityEngine;

namespace CodeLyokoFanGame
{
    // Player options saved between sessions. The options menu arrives in F10; for now the debug menu edits them.
    public static class GameSettings
    {
        private const string SensitivityKey = "settings.lookSensitivity";
        private const string InvertYKey = "settings.invertY";
        private const string DamageNumbersKey = "settings.damageNumbers";

        private static bool loaded;
        private static float lookSensitivity = 1f;
        private static bool invertY;
        private static bool showDamageNumbers = true;

        public static float LookSensitivity
        {
            get
            {
                Load();
                return lookSensitivity;
            }
            set
            {
                Load();
                lookSensitivity = Mathf.Clamp(value, 0.1f, 5f);
                PlayerPrefs.SetFloat(SensitivityKey, lookSensitivity);
            }
        }

        public static bool InvertY
        {
            get
            {
                Load();
                return invertY;
            }
            set
            {
                Load();
                invertY = value;
                PlayerPrefs.SetInt(InvertYKey, value ? 1 : 0);
            }
        }

        public static bool ShowDamageNumbers
        {
            get
            {
                Load();
                return showDamageNumbers;
            }
            set
            {
                Load();
                showDamageNumbers = value;
                PlayerPrefs.SetInt(DamageNumbersKey, value ? 1 : 0);
            }
        }

        private static void Load()
        {
            if (loaded)
            {
                return;
            }

            lookSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
            invertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;
            showDamageNumbers = PlayerPrefs.GetInt(DamageNumbersKey, 1) == 1;
            loaded = true;
        }
    }
}
