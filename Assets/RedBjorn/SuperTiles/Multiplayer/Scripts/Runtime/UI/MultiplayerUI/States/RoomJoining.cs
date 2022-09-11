using RedBjorn.SuperTiles.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI during room join process
    /// </summary>
    public class RoomJoining : MultiplayerUIState
    {
        public string Id;

        public override void OnEnter()
        {
            Controller.InProccess.SetActive(true);
            if (string.IsNullOrEmpty(Id))
            {
                NetworkController.RoomJoinRandom();
            }
            else
            {
                NetworkController.RoomJoin(Id);
            }
        }

        public override void OnExit()
        {
            Controller.InProccess.SetActive(false);
        }

        public override void OnRoomJoined()
        {
            ChangeState(new InRoom());
        }

        public override void OnRoomJoinFailed()
        {
            var id = string.IsNullOrEmpty(Id) ? "RANDOM" : Id;
            var text = $"Can't join {id} room";
            ConfirmMessageUI.Show(text, "OK", null, () => ChangeState(new InLobby()), null);
        }
    }
}
