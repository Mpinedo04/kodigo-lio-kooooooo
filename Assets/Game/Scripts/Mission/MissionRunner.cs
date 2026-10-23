using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeLyokoFanGame.Data;

namespace CodeLyokoFanGame
{
    // Plays a MissionDefinition step by step in the current level.
    public sealed class MissionRunner : MonoBehaviour
    {
        [SerializeField] private MissionDefinition mission;
        [SerializeField] private TowerObjective tower;
        [SerializeField] private CharacterSwitcher switcher;
        [SerializeField] private AelitaCompanion aelita;
        [SerializeField] private InputReader input;

        private readonly Dictionary<string, MissionMarker> markers = new Dictionary<string, MissionMarker>();
        private readonly Dictionary<string, Checkpoint> checkpoints = new Dictionary<string, Checkpoint>();
        private int stepIndex = -1;
        private int checkpointStep = -1;
        private float stepTimer;

        public MissionDefinition Mission => mission;
        public int CurrentStepIndex => stepIndex;
        public MissionPhase Phase { get; private set; } = MissionPhase.Intro;
        public string CurrentObjectiveText { get; private set; } = string.Empty;
        public int RemainingEnemies { get; private set; }
        public bool ShowsEnemyCount => CurrentStep != null && CurrentStep.condition == MissionStepCondition.EnemiesAtMost;

        private MissionStep CurrentStep => mission != null && stepIndex >= 0 && stepIndex < mission.steps.Count ? mission.steps[stepIndex] : null;

        public void Configure(MissionDefinition missionDefinition, TowerObjective missionTower, CharacterSwitcher characterSwitcher, InputReader reader, AelitaCompanion companion = null)
        {
            mission = missionDefinition;
            tower = missionTower;
            switcher = characterSwitcher;
            input = reader;
            aelita = companion;
        }

        public void JumpToStep(int index)
        {
            if (mission == null || index < 0 || index >= mission.steps.Count)
            {
                return;
            }

            stepIndex = index;
            stepTimer = 0f;
            MissionStep step = mission.steps[index];
            SetPhase(step.phase, step.objective);

            if (!string.IsNullOrEmpty(step.checkpointId) && checkpoints.TryGetValue(step.checkpointId, out Checkpoint checkpoint) && checkpoint != null)
            {
                checkpoint.Activate();
                checkpointStep = index;
            }
        }

        private void Start()
        {
            MissionMarker[] found = FindObjectsOfType<MissionMarker>();
            for (int i = 0; i < found.Length; i++)
            {
                if (!string.IsNullOrEmpty(found[i].Id))
                {
                    markers[found[i].Id] = found[i];
                }
            }

            Checkpoint[] points = FindObjectsOfType<Checkpoint>();
            for (int i = 0; i < points.Length; i++)
            {
                if (!string.IsNullOrEmpty(points[i].Id))
                {
                    checkpoints[points[i].Id] = points[i];
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMission(mission);
            }

            JumpToStep(0);
        }

        private void Update()
        {
            if (Phase == MissionPhase.Failed)
            {
                if (input != null && input.RetryPressed)
                {
                    RetryFromCheckpoint();
                }

                return;
            }

            if (Phase != MissionPhase.Complete && switcher != null && !switcher.HasAliveCharacter)
            {
                Fail(mission != null ? mission.failedObjective : "Mision fallida.");
                return;
            }

            if (Phase != MissionPhase.Complete && IsAelitaLost())
            {
                Fail(mission != null ? mission.aelitaLostObjective : "Aelita ha sido devirtualizada.");
                return;
            }

            MissionStep step = CurrentStep;
            if (step == null)
            {
                return;
            }

            stepTimer += Time.deltaTime;

            if (step.condition == MissionStepCondition.EnemiesAtMost)
            {
                RemainingEnemies = CountEnemies();
            }

            if (stepTimer >= step.minSeconds && IsStepDone(step))
            {
                AdvanceStep();
            }
        }

        private void Fail(string objective)
        {
            SetPhase(MissionPhase.Failed, objective);
            GameManager.RaiseEvent(GameEventId.MissionFailed);
        }

        private bool IsAelitaLost()
        {
            return aelita != null && aelita.TryGetComponent(out Health aelitaHealth) && aelitaHealth.IsDead;
        }

        private void AdvanceStep()
        {
            if (stepIndex + 1 >= mission.steps.Count)
            {
                return;
            }

            JumpToStep(stepIndex + 1);

            if (Phase == MissionPhase.Complete)
            {
                GameManager.RaiseEvent(GameEventId.MissionComplete);
            }
        }

        private bool IsStepDone(MissionStep step)
        {
            switch (step.condition)
            {
                case MissionStepCondition.Immediate:
                    return true;
                case MissionStepCondition.ReachMarker:
                    return IsPlayerAtMarker(step.markerId);
                case MissionStepCondition.EnemiesAtMost:
                    return RemainingEnemies <= step.enemyThreshold;
                case MissionStepCondition.TowerCleared:
                    return tower != null && tower.IsCleared;
                default:
                    return false;
            }
        }

        private bool IsPlayerAtMarker(string markerId)
        {
            if (!markers.TryGetValue(markerId ?? string.Empty, out MissionMarker marker) || marker == null)
            {
                return true;
            }

            PlayerController player = switcher != null ? switcher.ActiveCharacter : null;
            return player != null && player.gameObject.activeInHierarchy
                && Vector3.Distance(player.transform.position, marker.transform.position) <= marker.Radius;
        }

        private void SetPhase(MissionPhase phase, string objective)
        {
            Phase = phase;
            CurrentObjectiveText = objective;
            Debug.Log("[Mission] " + phase + ": " + objective);
        }

        // Emergency Return to the Past (Biblia 13.4): the team and Aelita come back at the last checkpoint,
        // the step it belongs to restarts, and X.A.N.A. grows stronger. Monsters already beaten stay beaten.
        public void RetryFromCheckpoint()
        {
            if (Phase != MissionPhase.Failed)
            {
                return;
            }

            if (checkpointStep < 0 || switcher == null)
            {
                RestartMission();
                return;
            }

            Vector3 fallback = switcher.Characters.Count > 0 && switcher.Characters[0] != null ? switcher.Characters[0].transform.position : transform.position;
            Vector3 respawn = CheckpointRegistry.GetRespawnPosition(fallback);
            Quaternion facing = CheckpointRegistry.ActiveCheckpoint != null ? CheckpointRegistry.ActiveCheckpoint.transform.rotation : Quaternion.identity;
            switcher.ReviveAllAt(respawn, facing);

            if (aelita != null)
            {
                aelita.gameObject.SetActive(true);
                if (aelita.TryGetComponent(out Health aelitaHealth))
                {
                    aelitaHealth.ResetHealth();
                }

                TeamTeleport.Move(aelita, TeamTeleport.Spot(respawn, facing, switcher.Characters.Count));
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RaiseXanaLevel();
            }

            Debug.Log("[Mission] Retorno de emergencia al punto de control del paso " + checkpointStep);
            JumpToStep(checkpointStep);
        }

        public void RestartMission()
        {
            Retry();
        }

        private static void Retry()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.ReloadCurrentLevel();
                return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private static int CountEnemies()
        {
            EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
            int alive = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                Health health = enemies[i].GetComponent<Health>();
                if (health == null || !health.IsDead)
                {
                    alive++;
                }
            }

            return alive;
        }
    }
}
