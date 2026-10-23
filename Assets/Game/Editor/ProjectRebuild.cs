using UnityEditor;
using UnityEngine;

namespace CodeLyokoFanGame.EditorTools
{
    public static class ProjectRebuild
    {
        // Runs every generator in dependency order. Also usable from the command line with -executeMethod.
        [MenuItem("Tools/Code Lyoko Fan Game/Rebuild Everything", priority = 0)]
        public static void RebuildEverything()
        {
            UrpPipelineSetup.EnsurePipeline();
            LookBuilder.EnsureShadersIncluded();
            PhysicsLayersSetup.EnsureLayers();
            CoreAssetsBuilder.CreateCoreAssets();
            PrototypeDataAssetBuilder.CreatePrototypeDataAssets();
            EnemyDataBuilder.CreateEnemyData();
            FullRosterPlaceholderBuilder.CreateFullRosterPlaceholders();
            BootSceneBuilder.BuildBootScene();
            MovementGymBuilder.BuildMovementGym();
            CombatArenaBuilder.BuildCombatArena();
            VehicleTrackBuilder.BuildVehicleTrack();
            EnemyGymBuilder.BuildEnemyGym();
            CodeLyokoSceneBuilder.BuildScene();
            Debug.Log("Rebuild Everything finished.");
        }
    }
}
