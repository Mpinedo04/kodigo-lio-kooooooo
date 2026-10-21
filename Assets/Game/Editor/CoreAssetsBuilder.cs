using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeLyokoFanGame.EditorTools
{
    // Creates the shared assets every scene needs: the InputReader and the game event channels.
    public static class CoreAssetsBuilder
    {
        private const string InputFolder = "Assets/Game/Input";
        private const string ControlsPath = InputFolder + "/LyokoControls.inputactions";
        private const string InputReaderPath = InputFolder + "/InputReader.asset";
        private const string EventsFolder = "Assets/Game/Data/Events";
        private const string EventSetPath = EventsFolder + "/GameEventSet.asset";

        [MenuItem("Tools/Code Lyoko Fan Game/Create Core Assets (Input + Events)")]
        public static void CreateCoreAssets()
        {
            EnsureInputReader();
            EnsureEventSet();
            AssetDatabase.SaveAssets();
            Debug.Log("Core assets ready: " + InputReaderPath + ", " + EventSetPath);
        }

        public static InputReader EnsureInputReader()
        {
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            if (controls == null)
            {
                Debug.LogError("Missing input actions asset: " + ControlsPath);
            }

            InputReader reader = AssetDatabase.LoadAssetAtPath<InputReader>(InputReaderPath);
            if (reader == null)
            {
                Directory.CreateDirectory(InputFolder);
                reader = ScriptableObject.CreateInstance<InputReader>();
                AssetDatabase.CreateAsset(reader, InputReaderPath);
            }

            reader.SetActions(controls);
            EditorUtility.SetDirty(reader);
            return reader;
        }

        public static GameEventSet EnsureEventSet()
        {
            Directory.CreateDirectory(EventsFolder);

            GameEventSet set = AssetDatabase.LoadAssetAtPath<GameEventSet>(EventSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<GameEventSet>();
                AssetDatabase.CreateAsset(set, EventSetPath);
            }

            set.playerDevirtualized = EnsureEvent("OnPlayerDevirtualized");
            set.enemyKilled = EnsureEvent("OnEnemyKilled");
            set.towerActivated = EnsureEvent("OnTowerActivated");
            set.towerDeactivated = EnsureEvent("OnTowerDeactivated");
            set.missionFailed = EnsureEvent("OnMissionFailed");
            set.missionComplete = EnsureEvent("OnMissionComplete");
            EditorUtility.SetDirty(set);
            return set;
        }

        private static GameEvent EnsureEvent(string eventName)
        {
            string path = EventsFolder + "/" + eventName + ".asset";
            GameEvent gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(path);
            if (gameEvent == null)
            {
                gameEvent = ScriptableObject.CreateInstance<GameEvent>();
                AssetDatabase.CreateAsset(gameEvent, path);
            }

            return gameEvent;
        }
    }
}
