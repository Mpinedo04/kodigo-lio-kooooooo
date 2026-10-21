using UnityEngine;
using UnityEngine.Events;

namespace CodeLyokoFanGame
{
    public sealed class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent gameEvent;
        [SerializeField] private UnityEvent response = new UnityEvent();

        private void OnEnable()
        {
            if (gameEvent != null)
            {
                gameEvent.Register(this);
            }
        }

        private void OnDisable()
        {
            if (gameEvent != null)
            {
                gameEvent.Unregister(this);
            }
        }

        public void OnEventRaised()
        {
            response.Invoke();
        }
    }
}
