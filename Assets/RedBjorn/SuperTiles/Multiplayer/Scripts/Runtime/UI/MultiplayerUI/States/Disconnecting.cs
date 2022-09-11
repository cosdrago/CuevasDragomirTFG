namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI during network disconnect process
    /// </summary>
    public class Disconnecting : MultiplayerUIState
    {
        public override void OnEnter()
        {
            Controller.InProccess.SetActive(true);
            var connected = Controller.Connected;
            connected.Hide();

            if (NetworkController.IsConnected)
            {
                NetworkController.Disconnect();
            }
            else
            {
                ChangeState(new Login());
            }
        }

        public override void OnExit()
        {
            Controller.IsConnected = false;
            Controller.InProccess.SetActive(false);
        }

        public override void OnDisconnected()
        {
            ChangeState(new Login());
        }
    }
}
