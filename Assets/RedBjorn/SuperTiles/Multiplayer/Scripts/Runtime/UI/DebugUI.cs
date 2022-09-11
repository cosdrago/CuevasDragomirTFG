using TMPro;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// Panel which displays network debug info
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        public bool ShowDebug;
        public TextMeshProUGUI Text;

        public void Show()
        {
            gameObject.SetActive(ShowDebug);
            if (gameObject.activeSelf)
            {
                Text.text = NetworkController.DebugInfo();
            }
        }
    }
}
