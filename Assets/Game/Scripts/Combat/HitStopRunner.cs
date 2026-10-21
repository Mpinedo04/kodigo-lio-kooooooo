using UnityEngine;

namespace CodeLyokoFanGame
{
    // Created on demand by HitStop; restores time once the freeze is over.
    public sealed class HitStopRunner : MonoBehaviour
    {
        private float savedTimeScale = 1f;

        public void RememberTimeScale(float timeScale)
        {
            savedTimeScale = timeScale > 0f ? timeScale : 1f;
        }

        private void Update()
        {
            if (HitStop.Active && Time.unscaledTime >= HitStop.EndsAt)
            {
                HitStop.End(savedTimeScale);
            }
        }
    }
}
