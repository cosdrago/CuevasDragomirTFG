using RedBjorn.SuperTiles.Multiplayer.Sdk.Local;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Fallback network SDK which is similiar to singleplayer regime
    /// </summary>
    public class LocalSdk : INetworkSdk
    {
        public class RoomEntity
        {
            public RedBjorn.SuperTiles.Multiplayer.RoomEntity Room;
            public RoomLobbyEntity Lobby;

            public override string ToString()
            {
                var roomName = Room == null ? "None" : Room.Name;
                var lobbyId = Lobby == null ? "None" : Lobby.Id;
                return $"Room Name: {roomName} (lobby Id: {lobbyId})";
            }
        }

        bool IsConnectedInternal;
        NetworkController Controller;
        Player Player;
        List<RoomEntity> RoomsOpened = new List<RoomEntity>();
        List<RoomEntity> RoomsClosed = new List<RoomEntity>();

        public string ServerDefault
        {
            get
            {
                return "Local PC";
            }
            set
            {

            }
        }

        public string ServerCurrent => "Local PC";
        public bool IsConnected => IsConnectedInternal;
        public bool IsInRoom => IsConnectedInternal && Controller.CurrentRoom != null;
        public double Time => System.Convert.ToDouble(System.DateTime.UtcNow.Second);
        public int Id => Player != null ? Player.Id : NetworkController.InvalidId;
        public string[] Servers => new string[1] { "Local PC" };

        public void Init(NetworkController controller)
        {
            Controller = controller;
        }

        public void Destroy()
        {
            RoomsClosed.Clear();
            RoomsOpened.Clear();
            Controller = null;
            Player = null;
        }

        public void Connect()
        {
            IsConnectedInternal = true;
            Player = new Player();
            Controller.Player = Player;
            Controller.ConnectionTargets.ForEach(t => t.OnConnected());
        }

        public void Disconnect()
        {
            IsConnectedInternal = false;
            RoomsOpened.Clear();
            RoomsClosed.Clear();
            Player = null;
            Controller.ConnectionTargets.ToList().ForEach(t => t.OnDisconnected());
        }

        public string DebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"IsConnected: {IsConnectedInternal}");
            if (Player == null)
            {
                sb.AppendLine($"Player: Null");
            }
            else
            {
                sb.AppendLine($"Player: {Player.ToString()}");
            }
            sb.AppendLine("\nRooms (opened):");
            foreach (var room in RoomsOpened)
            {
                sb.AppendLine($"{room.ToString()}");
            }
            sb.AppendLine("\nRooms (closed):");
            foreach (var room in RoomsClosed)
            {
                sb.AppendLine($"{room.ToString()}");
            }
            if (Controller.CurrentRoom != null)
            {
                sb.AppendLine();
                sb.AppendLine(Controller.CurrentRoom.ToString());
            }
            return sb.ToString();
        }

        public bool IsMaster(INetworkPlayer player)
        {
            return player != null && Player == player;
        }

        public bool IsMaster()
        {
            return Player != null ? Player.IsMasterClient : false;
        }

        public void LobbyJoin()
        {
            if (!IsConnectedInternal)
            {
                return;
            }

            foreach (var target in Controller.LobbyRoomTargets)
            {
                target.OnRoomListUpdate();
            }
        }

        public void RoomCreate(RedBjorn.SuperTiles.Multiplayer.RoomEntity room)
        {
            var lobby = new RoomLobbyEntity { Id = room.Name, TurnDuration = room.TurnDuration };
            var localRoom = new RoomEntity() { Room = room, Lobby = lobby };
            RoomsOpened.Add(localRoom);
            Controller.LobbyRooms.Add(lobby);
            Controller.RoomTargets.ForEach(r => r.OnCreated());
            RoomJoinInternal(localRoom);
        }

        public void RoomJoin(string id)
        {
            var room = RoomsOpened.FirstOrDefault(r => r.Lobby.Id == id);
            if (room != null)
            {
                RoomJoinInternal(room);
            }
            else
            {
                Controller.RoomTargets.ForEach(r => r.OnJoinedFailed());
            }
        }

        public void RoomJoinRandom()
        {
            if (RoomsOpened.Count == 0)
            {
                Controller.RoomTargets.ForEach(r => r.OnJoinedFailed());
                return;
            }

            var index = Random.Range(0, RoomsOpened.Count);
            RoomJoinInternal(RoomsOpened[index]);
        }

        public void RoomLeave()
        {
            if (Controller.CurrentRoom == null)
            {
                Log.E("Failed to leave room. Was not in room");
                return;
            }
            Controller.CurrentRoom.PlayerRemove(Id);
            if (Controller.CurrentRoom.Players.Count == 0)
            {
                var local = RoomsOpened.FirstOrDefault(r => r.Room == Controller.CurrentRoom);
                if (local != null)
                {
                    Controller.LobbyRooms.Remove(local.Lobby);
                    RoomsOpened.Remove(local);
                    RoomsClosed.Remove(local);
                }
            }
            Controller.CurrentRoom = null;
            Controller.RoomTargets.ForEach(r => r.OnLeft());
        }

        void RoomJoinInternal(RoomEntity room)
        {
            Controller.CurrentRoom = room.Room;
            Controller.CurrentRoom.PlayerAdd(Player);
            Controller.RoomTargets.ForEach(r => r.OnJoined());
        }

        public void SendRoomSlotChangeType(int slot, int type)
        {
            if (Controller.CurrentRoom == null)
            {
                Log.E("Can't change slot type. Not in Room");
                return;
            }

            Controller.CurrentRoom.Slots[slot].Type = SquadControllerData.Type(type);
            Controller.InRoomTargets.ForEach(r => r.OnRoomPropertiesUpdate());
        }

        public void SendRoomSlotChangeOwner(int slot, int owner)
        {
            if (Controller.CurrentRoom == null)
            {
                Log.E("Can't change slot owner. Not in Room");
                return;
            }

            foreach (var s in Controller.CurrentRoom.Slots)
            {
                if (s.Player != null && s.Player.Id == owner)
                {
                    s.Player = null;
                }
            }
            Controller.CurrentRoom.Slots[slot].Player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == owner);
            Controller.InRoomTargets.ForEach(r => r.OnRoomPropertiesUpdate());
        }

        public void SendIsReady()
        {
            if (Controller.CurrentRoom == null)
            {
                Log.E("Can't set ready. Not in Room");
                return;
            }

            if (Controller.CurrentRoom.Ready.Contains(Player))
            {
                Controller.CurrentRoom.Ready.Remove(Player);
            }
            else
            {
                Controller.CurrentRoom.Ready.Add(Player);
            }
            Controller.InRoomTargets.ForEach(r => r.OnRoomPropertiesUpdate());
        }

        public void SendGameStart()
        {
            var local = RoomsOpened.FirstOrDefault(r => r.Room == Controller.CurrentRoom);
            if (local != null)
            {
                Controller.LobbyRooms.Remove(local.Lobby);
                RoomsOpened.Remove(local);
                RoomsClosed.Add(local);
            }
            Controller.LoadLevel();
        }

        public void SendBattleStart()
        {
            StartTurn();
        }

        public void SendBattleIsLoaded()
        {
            if (Controller.CurrentRoom == null)
            {
                Log.E("Can't set is loaded. Not in Room");
                return;
            }

            Controller.CurrentRoom.Loaded.Add(Player);
            Controller.InRoomTargets.ForEach(r => r.OnRoomPropertiesUpdate());
        }

        public void SendBattleAction(byte[] actionSerialized)
        {
            foreach (var target in Controller.BattleActionTargets)
            {
                target.OnBattleAction(actionSerialized);
            }
        }

        public void SendTurnFinishLaunch(int turn)
        {
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnFinishLaunched(turn);
            }
        }

        public void SendTurnFinishCompleted(int turn)
        {
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnFinishCompleted(Player, turn);
            }
        }

        public void Error(string message)
        {
            Log.E(message);
        }

        void StartTurn()
        {
            foreach (var target in Controller.TurnTargets)
            {
                target.OnTurnStart(Time);
            }
        }
    }
}
