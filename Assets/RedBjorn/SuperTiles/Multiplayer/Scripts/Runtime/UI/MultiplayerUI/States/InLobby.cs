using RedBjorn.Utils;
using System.Linq;
using TMPro;

namespace RedBjorn.SuperTiles.Multiplayer.UI.Multiplayer.States
{
    /// <summary>
    /// Multiplayer UI state which displays UI when player is in a lobby
    /// </summary>
    public class InLobby : MultiplayerUIState
    {
        public override void OnEnter()
        {
            var connected = Controller.Connected;
            connected.Show();
            connected.Profile.Refresh();
            connected.Lobby.Show();

            LevelsCreate();
            TurnDurationCreate();

            var create = connected.Lobby.Create;
            create.onClick.RemoveAllListeners();
            create.onClick.AddListener(() => ChangeState(new RoomCreation()));
            create.interactable = true;
            create.gameObject.SetActive(true);

            var join = connected.Lobby.RoomJoin;
            join.onClick.RemoveAllListeners();
            join.onClick.AddListener(() => ChangeState(new RoomJoining()));
            join.interactable = true;
            join.gameObject.SetActive(true);

            if (NetworkController.IsConnected)
            {
                NetworkController.LobbyJoin();
            }
        }

        public override void OnExit()
        {
            var lobby = Controller.Connected.Lobby;
            lobby.RoomJoin.interactable = false;
            lobby.Create.interactable = false;
            lobby.Hide();
            Controller.InProccess.SetActive(false);
        }

        public override void OnConnected()
        {
            NetworkController.LobbyJoin();
        }

        public override void OnRoomListUpdate()
        {
            RoomsUpdate();
        }

        void RoomsUpdate()
        {
            var rooms = Controller.Connected.Lobby.RoomJoins;
            for (int i = rooms.Count - 1; i >= 0; i--)
            {
                Spawner.Despawn(rooms[i].gameObject);
            }
            rooms.Clear();

            var roomJoinRef = Controller.Connected.Lobby.RoomJoinRef;
            foreach (var room in NetworkController.GetLobbyRooms())
            {
                var ui = Spawner.Spawn(roomJoinRef, roomJoinRef.transform.parent);
                ui.onClick.RemoveAllListeners();
                ui.onClick.AddListener(() => ChangeState(new RoomJoining() { Id = room.Id }));
                ui.gameObject.SetActive(true);
                ui.GetComponentInChildren<TextMeshProUGUI>().text = room.Description;
                rooms.Add(ui);
            }
        }

        void LevelsCreate()
        {
            var toggles = Controller.Connected.Lobby.Levels;
            for (int i = toggles.Count - 1; i >= 0; i--)
            {
                toggles[i].group = null;
                Spawner.Despawn(toggles[i].gameObject);
            }
            toggles.Clear();
            var levels = Controller.Levels;
            var toggleRef = Controller.Connected.Lobby.LevelRef;
            for (int i = 0; i < levels.Count; i++)
            {
                var ui = Spawner.Spawn(toggleRef, toggleRef.transform.parent);
                ui.isOn = i == 0;
                ui.gameObject.SetActive(true);
                var text = ui.GetComponentInChildren<TextMeshProUGUI>(true);
                if (text)
                {
                    text.text = levels[i].Caption;
                }
                ui.group = Controller.Connected.Lobby.LevelGroup;
                toggles.Add(ui);
            }
        }

        void TurnDurationCreate()
        {
            var turnDuration = Controller.Connected.Lobby.TurnDuration;
            turnDuration.options.Clear();
            var durations = S.Network.TurnDurations;
            var valueDefault = 0;
            var durationDefault = durations[valueDefault];
            var counter = 0;
            foreach (var duration in durations.OrderBy(t => t))
            {
                turnDuration.options.Add(new TMP_Dropdown.OptionData { text = duration.ToString() });
                if (duration == durationDefault)
                {
                    valueDefault = counter;
                }
                counter++;
            }
            turnDuration.value = valueDefault;
        }
    }
}
