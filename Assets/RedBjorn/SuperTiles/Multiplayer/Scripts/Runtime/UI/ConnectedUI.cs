using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel to be shown when network connection is established
    /// </summary>
    public class ConnectedUI : MonoBehaviour
    {
        public RoomUI Room;
        public LobbyUI Lobby;
        public ProfileUI Profile;
        public TextMeshProUGUI Server;
        public Button QuitButton;

        public void Show()
        {
            Server.text = NetworkController.ServerCurrent;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Room.Hide();
            Lobby.Hide();
            gameObject.SetActive(false);
        }
    }
}
