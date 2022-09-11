using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// MonoBehaviour which provides Unity callbacks to NetworkController
    /// </summary>
    public class NetworkControllerView : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            NetworkController.Destroy();
        }
    }
}
