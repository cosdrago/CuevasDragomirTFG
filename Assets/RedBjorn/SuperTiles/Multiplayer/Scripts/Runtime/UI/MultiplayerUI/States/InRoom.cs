namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI when player is in a room
    /// </summary>
    public class InRoom : MultiplayerUIState
    {
        public override void OnEnter()
        {
            var connected = Controller.Connected;
            connected.Show();
            connected.Profile.Refresh();
            connected.Room.Show(NetworkController.GetCurrentRoom(), Controller);
        }

        public override void OnExit()
        {
            Controller.Connected.Room.Hide();
        }

        public override void OnRoomLeft()
        {
            ChangeState(new InLobby());
        }
    }
}
