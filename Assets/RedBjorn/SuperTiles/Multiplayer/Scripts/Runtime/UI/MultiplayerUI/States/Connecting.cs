namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI during network connect process
    /// </summary>
    public class Connecting : MultiplayerUIState
    {
        public override void OnEnter()
        {
            Controller.Connected.QuitButton.onClick.RemoveAllListeners();
            Controller.IsConnected = NetworkController.IsConnected;
            if (Controller.IsConnected)
            {
                ShowConnected();
            }
            else
            {
                Controller.InProccess.SetActive(true);
                NetworkController.Connect();
            }
        }

        public override void OnExit()
        {
            Controller.InProccess.SetActive(false);
        }

        public override void OnConnected()
        {
            Controller.IsConnected = true;
            ShowConnected();
        }

        void ShowConnected()
        {
            var connected = Controller.Connected;
            connected.Show();
            connected.QuitButton.onClick.AddListener(() => ChangeState(new Disconnecting()));
            connected.Profile.Refresh();
            if (NetworkController.IsInRoom)
            {
                ChangeState(new InRoom());
            }
            else
            {
                ChangeState(new InLobby());
            }
        }
    }
}
