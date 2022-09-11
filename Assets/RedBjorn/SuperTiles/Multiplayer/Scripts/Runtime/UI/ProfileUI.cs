using TMPro;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI profile elements
    /// </summary>
    public class ProfileUI : MonoBehaviour
    {
        public TextMeshProUGUI Nickname;

        public void Refresh()
        {
            Nickname.text = NetworkController.LocalPlayer.Nickname;
        }
    }
}
