using RedBjorn.SuperTiles.UI;
using TMPro;

namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI during room creation process
    /// </summary>
    public class RoomCreation : MultiplayerUIState
    {
        public override void OnEnter()
        {
            Controller.InProccess.SetActive(true);

            var lobby = Controller.Connected.Lobby;
            var roomName = lobby.RoomName.text;
            if (string.IsNullOrEmpty(roomName))
            {
                roomName = lobby.RoomName.placeholder.GetComponent<TextMeshProUGUI>().text;
            }
            lobby.RoomName.text = string.Empty;
            var index = lobby.Levels.FindIndex(t => t.isOn);
            var level = Controller.Levels[index];
            var network = S.Network;
            var durations = network.TurnDurations;
            var durationText = lobby.TurnDuration.options[lobby.TurnDuration.value].text;
            float duration = 0;
            if (!float.TryParse(durationText, out duration))
            {
                duration = durations[0];
            }
            NetworkController.RoomCreate(new RoomEntity(roomName, index, level, duration));
        }

        public override void OnExit()
        {
            Controller.InProccess.SetActive(false);
        }

        public override void OnRoomJoined()
        {
            ChangeState(new InRoom());
        }

        public override void OnRoomCreateFailed()
        {
            var text = $"Can't create room";
            ConfirmMessageUI.Show(text, "OK", null, () => ChangeState(new InLobby()), null);
        }

        public override void OnRoomJoinFailed()
        {
            var text = $"Can't join room";
            ConfirmMessageUI.Show(text, "OK", null, () => ChangeState(new InLobby()), null);
        }
    }
}
