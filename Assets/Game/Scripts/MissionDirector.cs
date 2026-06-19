using UnityEngine;

namespace CodeLyokoFanGame
{
    public sealed class MissionDirector : MonoBehaviour
    {
        [SerializeField] private TowerObjective tower;
        [SerializeField] private AelitaCompanion aelita;
        [SerializeField] private Transform bridgeMarker;
        [SerializeField] private Transform towerMarker;
        [SerializeField] private float markerReachedDistance = 6f;

        public MissionPhase Phase { get; private set; } = MissionPhase.Intro;
        public string CurrentObjectiveText { get; private set; } = "Jeremie: X.A.N.A. ha activado una torre. Entrad en Lyoko.";

        private void Update()
        {
            GameObject player = GetActivePlayer();
            if (player == null)
            {
                return;
            }

            switch (Phase)
            {
                case MissionPhase.Intro:
                    SetPhase(MissionPhase.ReachBridge, "Llega al puente de datos y prueba los vehiculos.");
                    break;
                case MissionPhase.ReachBridge:
                    if (bridgeMarker == null || Vector3.Distance(player.transform.position, bridgeMarker.position) <= markerReachedDistance)
                    {
                        SetPhase(MissionPhase.ClearCombatPlateau, "Limpia la plataforma de combate.");
                    }
                    break;
                case MissionPhase.ClearCombatPlateau:
                    if (CountEnemies() <= 1)
                    {
                        SetPhase(MissionPhase.EscortAelita, "Escolta a Aelita hasta la torre activada.");
                    }
                    break;
                case MissionPhase.EscortAelita:
                    if (towerMarker == null || Vector3.Distance(player.transform.position, towerMarker.position) <= markerReachedDistance)
                    {
                        SetPhase(MissionPhase.DefeatBoss, "Derrota al Megatank y protege a Aelita.");
                    }
                    break;
                case MissionPhase.DefeatBoss:
                    if (CountEnemies() == 0)
                    {
                        SetPhase(MissionPhase.DeactivateTower, "Manten E cerca de la torre para introducir CODE: LYOKO.");
                    }
                    break;
                case MissionPhase.DeactivateTower:
                    if (tower != null && tower.IsCleared)
                    {
                        SetPhase(MissionPhase.Complete, "Torre desactivada. Retorno al pasado listo.");
                    }
                    break;
            }
        }

        public void SetTower(TowerObjective newTower)
        {
            tower = newTower;
        }

        public void SetAelita(AelitaCompanion companion)
        {
            aelita = companion;
        }

        private void SetPhase(MissionPhase phase, string objective)
        {
            Phase = phase;
            CurrentObjectiveText = objective;
        }

        private static GameObject GetActivePlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].activeInHierarchy)
                {
                    return players[i];
                }
            }

            return null;
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
