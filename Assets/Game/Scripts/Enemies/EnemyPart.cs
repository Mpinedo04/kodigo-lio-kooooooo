using UnityEngine;

namespace CodeLyokoFanGame
{
    // A breakable body part with its own Health (Krabe legs). Breaking parts changes the monster's
    // behaviour; the part comes back when the monster gets up.
    [RequireComponent(typeof(Health))]
    public sealed class EnemyPart : MonoBehaviour
    {
        [SerializeField] private string partId = "Leg";

        private Health health;

        public string PartId => partId;
        public Health Health => health != null ? health : health = GetComponent<Health>();
        public bool IsBroken => Health.IsDead || !gameObject.activeSelf;

        public void Configure(string id, float maxHealth)
        {
            partId = id;
            Health.SetTeam(Team.Enemy);
            Health.SetDestroyOnDeath(false);
            Health.SetMaxHealth(maxHealth);
        }

        public void Restore()
        {
            gameObject.SetActive(true);
            Health.ResetHealth();
        }
    }
}
