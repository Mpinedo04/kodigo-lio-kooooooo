using UnityEngine;

namespace CodeLyokoFanGame
{
    // Remembers which prefab an instance came from so PrefabPool can take it back.
    public sealed class PooledObject : MonoBehaviour
    {
        public GameObject Prefab { get; set; }
    }
}
