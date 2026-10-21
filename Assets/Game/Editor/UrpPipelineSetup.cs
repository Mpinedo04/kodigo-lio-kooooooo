using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CodeLyokoFanGame.EditorTools
{
    public static class UrpPipelineSetup
    {
        private const string SettingsFolder = "Assets/Settings";
        private const string PipelinePath = SettingsFolder + "/URP_Pipeline.asset";
        private const string RendererPath = SettingsFolder + "/URP_Renderer.asset";
        private const string UrpPackagePath = "Packages/com.unity.render-pipelines.universal";
        private const string DefaultPostProcessDataPath = UrpPackagePath + "/Runtime/Data/PostProcessData.asset";

        [MenuItem("Tools/Code Lyoko Fan Game/Setup URP Pipeline")]
        public static void SetupFromMenu()
        {
            UniversalRenderPipelineAsset pipeline = EnsurePipeline();
            Debug.Log("URP pipeline active: " + AssetDatabase.GetAssetPath(pipeline));
        }

        public static UniversalRenderPipelineAsset EnsurePipeline()
        {
            if (GraphicsSettings.renderPipelineAsset is UniversalRenderPipelineAsset current)
            {
                return current;
            }

            Directory.CreateDirectory(SettingsFolder);

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
                if (rendererData == null)
                {
                    rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                    rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(DefaultPostProcessDataPath);
                    AssetDatabase.CreateAsset(rendererData, RendererPath);
                    ResourceReloader.ReloadAllNullIn(rendererData, UrpPackagePath);
                    EditorUtility.SetDirty(rendererData);
                }

                pipeline = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            // Quality levels without their own pipeline fall back to this default.
            GraphicsSettings.renderPipelineAsset = pipeline;
            AssetDatabase.SaveAssets();
            return pipeline;
        }
    }
}
