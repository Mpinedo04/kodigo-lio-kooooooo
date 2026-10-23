using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CodeLyokoFanGame.EditorTools
{
    // F09: screenshots of the generated scenes from fixed viewpoints, to check the look without playing.
    // Command line: -executeMethod CodeLyokoFanGame.EditorTools.ArtPreview.CaptureAll -captureDir <folder>
    public static class ArtPreview
    {
        private struct Shot
        {
            public string scene;
            public string name;
            public Vector3 position;
            public Vector3 target;
        }

        private static readonly Shot[] Shots =
        {
            new Shot { scene = ProjectScenes.ForestScenePath, name = "Forest_Start", position = new Vector3(4f, 3.2f, -19f), target = new Vector3(0f, 1.4f, -10f) },
            new Shot { scene = ProjectScenes.ForestScenePath, name = "Forest_Overview", position = new Vector3(0f, 16f, -36f), target = new Vector3(0f, 0f, 20f) },
            new Shot { scene = ProjectScenes.ForestScenePath, name = "Forest_Tower", position = new Vector3(9f, 5f, 33f), target = new Vector3(0f, 3f, 48f) },
            new Shot { scene = ProjectScenes.ForestScenePath, name = "Forest_TowerInterior", position = new Vector3(226f, 4.5f, 44f), target = new Vector3(220f, 1.5f, 50f) },
            new Shot { scene = CombatArenaBuilder.ScenePath, name = "Arena", position = new Vector3(0f, 9f, -26f), target = new Vector3(0f, 0.5f, 4f) },
            new Shot { scene = "Assets/Scenes/Test/EnemyGym.unity", name = "EnemyGym", position = new Vector3(0f, 10f, -22f), target = new Vector3(0f, 0f, 8f) },
            new Shot { scene = "Assets/Scenes/Test/VehicleTrack.unity", name = "VehicleTrack", position = new Vector3(0f, 18f, -40f), target = new Vector3(0f, 0f, 0f) }
        };

        [MenuItem("Tools/Code Lyoko Fan Game/Capture Art Preview")]
        public static void CaptureAll()
        {
            string folder = ArgumentAfter("-captureDir") ?? Path.Combine(Path.GetTempPath(), "LyokoCaptures");
            Directory.CreateDirectory(folder);

            string openScene = null;
            foreach (Shot shot in Shots)
            {
                if (!File.Exists(shot.scene))
                {
                    continue;
                }

                if (openScene != shot.scene)
                {
                    EditorSceneManager.OpenScene(shot.scene, OpenSceneMode.Single);
                    openScene = shot.scene;
                }

                Capture(Path.Combine(folder, shot.name + ".png"), shot.position, shot.target);
            }

            Debug.Log("[ArtPreview] Captures saved in " + folder);
        }

        // A temporary scene with every model in a row (warriors, monsters, vehicles) to check the art.
        public static void CaptureModels()
        {
            string folder = ArgumentAfter("-captureDir") ?? Path.Combine(Path.GetTempPath(), "LyokoCaptures");
            Directory.CreateDirectory(folder);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CodeLyokoSceneBuilder.CreateLighting();
            CodeLyokoSceneBuilder.CreatePlatform("ShowcaseFloor", new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 20f), CodeLyokoSceneBuilder.Material("Lyoko_Forest_Green", new Color(0.08f, 0.7f, 0.33f), 0.2f));

            LyokoCharacter[] warriors = { LyokoCharacter.Odd, LyokoCharacter.Ulrich, LyokoCharacter.Yumi, LyokoCharacter.Aelita };
            for (int i = 0; i < warriors.Length; i++)
            {
                GameObject holder = new GameObject(warriors[i].ToString());
                holder.transform.position = new Vector3(-2.4f + i * 1.6f, 0f, 0f);
                holder.transform.rotation = Quaternion.Euler(0f, 180f + (i - 1.5f) * 12f, 0f);
                WarriorModel.Build(warriors[i], holder.transform);
            }

            ShowcaseExtras.Invoke();
            Capture(Path.Combine(folder, "Models_Warriors.png"), new Vector3(0f, 1.5f, -4.6f), new Vector3(0f, 1f, 0f));
            Capture(Path.Combine(folder, "Models_Warriors_Side.png"), new Vector3(3.5f, 1.4f, -2.5f), new Vector3(-0.8f, 1f, 0f));
            Capture(Path.Combine(folder, "Models_Row.png"), new Vector3(0f, 6f, -14f), new Vector3(0f, 0.8f, 4f));
            Capture(Path.Combine(folder, "Models_Monsters.png"), new Vector3(0f, 3.5f, -6f), new Vector3(0f, 1.2f, 5f));
            Capture(Path.Combine(folder, "Models_Vehicles.png"), new Vector3(0f, 3f, 4f), new Vector3(0f, 1f, 10f));
            Debug.Log("[ArtPreview] Model captures saved in " + folder);
        }

        // Later art steps (monsters, vehicles) add their showcase pieces here.
        private static class ShowcaseExtras
        {
            public static void Invoke()
            {
                EnemyKind[] monsters = { EnemyKind.Kankrelat, EnemyKind.Hornet, EnemyKind.Blok, EnemyKind.Krab, EnemyKind.Megatank };
                for (int i = 0; i < monsters.Length; i++)
                {
                    float height = monsters[i] == EnemyKind.Hornet ? 2f : 0f;
                    EnemyAI monster = EnemyRigBuilder.Build(monsters[i].ToString(), monsters[i], new Vector3(-9f + i * 4.5f, height, 5f), null, null);
                    monster.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }

                VehicleStyle[] vehicles = { VehicleStyle.Overboard, VehicleStyle.Overbike, VehicleStyle.Overwing };
                Color[] colors = { new Color(0.45f, 0.18f, 0.95f), new Color(0.06f, 0.42f, 0.2f), new Color(0.02f, 0.02f, 0.025f) };
                for (int i = 0; i < vehicles.Length; i++)
                {
                    Material body = CodeLyokoSceneBuilder.Material(vehicles[i] == VehicleStyle.Overboard ? "Odd_Violet" : vehicles[i] == VehicleStyle.Overbike ? "Ulrich_Green" : "Yumi_Black", colors[i], vehicles[i] == VehicleStyle.Overboard ? 0.4f : 0.3f);
                    VehicleController prefab = CodeLyokoSceneBuilder.BuildVehiclePrefab(vehicles[i].ToString(), vehicles[i], body, PrimitiveType.Cube);
                    GameObject vehicle = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
                    vehicle.transform.SetPositionAndRotation(new Vector3(-5f + i * 5f, 1.1f, 10f), Quaternion.Euler(0f, 150f, 0f));
                }
            }
        }

        public static void Capture(string file, Vector3 position, Vector3 target)
        {
            GameObject cameraObject = new GameObject("ArtPreviewCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = position;
            camera.transform.LookAt(target);
            camera.fieldOfView = 55f;
            camera.farClipPlane = 600f;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            RenderTexture texture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = texture;
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            Texture2D image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            File.WriteAllBytes(file, image.EncodeToPNG());

            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }

        private static string ArgumentAfter(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == flag)
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
