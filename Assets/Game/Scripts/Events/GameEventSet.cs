using UnityEngine;

namespace CodeLyokoFanGame
{
    // The game-wide event channels, looked up by id through GameManager.RaiseEvent.
    [CreateAssetMenu(menuName = "Code Lyoko/Game Event Set", fileName = "GameEventSet")]
    public sealed class GameEventSet : ScriptableObject
    {
        public GameEvent playerDevirtualized;
        public GameEvent enemyKilled;
        public GameEvent towerActivated;
        public GameEvent towerDeactivated;
        public GameEvent missionFailed;
        public GameEvent missionComplete;

        public GameEvent Get(GameEventId id)
        {
            switch (id)
            {
                case GameEventId.PlayerDevirtualized: return playerDevirtualized;
                case GameEventId.EnemyKilled: return enemyKilled;
                case GameEventId.TowerActivated: return towerActivated;
                case GameEventId.TowerDeactivated: return towerDeactivated;
                case GameEventId.MissionFailed: return missionFailed;
                case GameEventId.MissionComplete: return missionComplete;
                default: return null;
            }
        }
    }
}
