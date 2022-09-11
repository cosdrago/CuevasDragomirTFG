#if PHOTON_UNITY_NETWORKING
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using RedBjorn.SuperTiles.Multiplayer.Sdk.Photon;
using RedBjorn.SuperTiles.Multiplayer.Sdk.Photon.Requests;
using RedBjorn.Utils;
using System.Collections.Generic;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Class which implements INetworkSdk to communicate with Photon Unity Networking SDK
    /// </summary>
    public partial class PhotonSdk : INetworkSdk
    {
        bool ConnectedToMaster;
        NetworkController Controller;
        List<Server> ServersInternal;
        Server ServerDefaultInternal;

        const string ServerUnknown = "Unknown";
        const string ServerCodeBestRegion = "br";

        public string ServerCurrent
        {
            get
            {
                var servername = ServerUnknown;
                var region = PhotonNetwork.CloudRegion;
                if (!string.IsNullOrEmpty(region))
                {
                    var slash = region.IndexOf("/");
                    if (slash >= 0)
                    {
                        region = region.Substring(0, slash);
                    }
                    var server = ServersInternal.FirstOrDefault(s => s.Code == region);
                    if (server != null)
                    {
                        servername = server.Name;
                    }
                }

                return servername;
            }
        }

        public string ServerDefault
        {
            get
            {
                return ServerDefaultInternal != null ? ServerDefaultInternal.Name : string.Empty;
            }
            set
            {
                ServerDefaultInternal = ServersInternal.FirstOrDefault(s => s.Name == value);
                ServerSave();
            }
        }

        public bool IsInRoom => PhotonNetwork.InRoom;
        public double Time => PhotonNetwork.Time;
        public int Id => PhotonNetwork.LocalPlayer.ActorNumber;
        public bool IsConnected => PhotonNetwork.IsConnectedAndReady;
        public string[] Servers => ServersInternal.Select(s => s.Name).ToArray();
        public NetworkSettings Network => S.Network;

        public PhotonSdk(List<Server> servers)
        {
            ServersInternal = servers;
            var saved = PlayerProfile.LoadString(SERVER_KEY);
            ServerDefaultInternal = ServersInternal.FirstOrDefault(s => s.Code == saved);
            if (ServerDefaultInternal == null && ServersInternal.Count > 0)
            {
                ServerDefaultInternal = ServersInternal[0];
                ServerSave();
            }
        }

        public void Init(NetworkController controller)
        {
            Controller = controller;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void Destroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Log.W("Player is already connected to Photon");
            }
            else
            {
                PhotonNetwork.LocalPlayer.NickName = PlayerProfile.Nickname;
                Log.I($"Try to connect as {PhotonNetwork.LocalPlayer.NickName}");
                var settings = new AppSettings();
                PhotonNetwork.PhotonServerSettings.AppSettings.CopyTo(settings);
                var regionCode = string.Empty;
                if (ServerDefaultInternal != null)
                {
                    regionCode = ServerDefaultInternal.Code;
                    if (regionCode == ServerCodeBestRegion)
                    {
                        regionCode = string.Empty;
                    }
                }
                settings.FixedRegion = regionCode;
                PhotonNetwork.ConnectUsingSettings(settings);
            }
        }

        public void Disconnect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Log.I("Disconnecting...");
                PhotonNetwork.Disconnect();
            }
        }

        public string DebugInfo()
        {
            var sb = new System.Text.StringBuilder();
            var inRoom = PhotonNetwork.CurrentRoom != null;
            var roomName = inRoom ? PhotonNetwork.CurrentRoom.Name : "None";
            sb.Append($"{PhotonNetwork.NetworkClientState}");
            if (PhotonNetwork.NetworkingClient != null)
            {
                sb.Append($"\n{PhotonNetwork.NetworkingClient.Server}");
            }
            sb.AppendLine();
            sb.AppendLine($"Master {ConnectedToMaster}");
            sb.AppendLine($"Lobby: {PhotonNetwork.InLobby}");
            sb.AppendLine($"Room: {roomName}\n");
            if (inRoom)
            {
                foreach (var prop in PhotonNetwork.CurrentRoom.CustomProperties)
                {
                    sb.AppendLine($"{prop.Key} - {prop.Value}");
                }
            }

            if (Controller.CurrentRoom != null)
            {
                sb.AppendLine();
                sb.AppendLine(Controller.CurrentRoom.ToString());
            }
            return sb.ToString();
        }

        public bool IsMaster()
        {
            return PhotonNetwork.IsMasterClient;
        }

        public bool IsMaster(INetworkPlayer player)
        {
            return player.IsMasterClient;
        }

        public void SendIsReady()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Log.E("Can't SendIsReady. Not in a room");
                return;
            }

            var setValue = true;
            var key = PlayerReadyKey(PhotonNetwork.LocalPlayer.ActorNumber);
            var val = PhotonNetwork.CurrentRoom.CustomProperties.TryGetOrDefault(key);
            if (val != null)
            {
                setValue = !(bool)val;
            }
            var prop = new Hashtable();
            prop.Add(key, setValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(prop);
        }

        public void SendBattleIsLoaded()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Log.E("Can't SendBattleIsLoaded. Not in a room");
                return;
            }

            var key = SessionLoadedKey(PhotonNetwork.LocalPlayer.ActorNumber);
            var prop = new Hashtable();
            prop.Add(key, true);
            PhotonNetwork.CurrentRoom.SetCustomProperties(prop);
        }

        public void LobbyJoin()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                Log.W("Won't connect to Lobby. Already in Lobby");
            }
        }

        public void RoomCreate(RoomEntity room)
        {
            var props = new Hashtable();
            props.Add(LEVEL_KEY, room.LevelIndex);
            props.Add(TURN_DURATION_KEY, room.TurnDuration);
            var slots = room.Slots;
            for (int slot = 0; slot < slots.Count; slot++)
            {
                props.Add(SlotGetOwnerKey(slot), NetworkController.InvalidId);
                props.Add(SlotGetTypeKey(slot), SquadControllerData.TypeIndex(SquadControllerType.Player));
            }
            var options = new RoomOptions();
            options.CustomRoomProperties = props;
            var id = room.Name;
            if (string.IsNullOrEmpty(id))
            {
                id = "Room";
            }
            options.CustomRoomPropertiesForLobby = new string[] { LEVEL_KEY, TURN_DURATION_KEY, PLAYERS_COUNT_KEY };
            id = $"{id} ({System.Guid.NewGuid().ToString()})";
            PhotonNetwork.CreateRoom(id, options);
        }

        public void RoomLeave()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        public void RoomJoin(string id)
        {
            PhotonNetwork.JoinRoom(id);
        }

        public void RoomJoinRandom()
        {
            if (PhotonNetwork.InRoom)
            {
                Log.W("Player is already in room");
            }
            else
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public void SendRoomSlotChangeType(int slot, int type)
        {
            if (!IsMaster())
            {
                return;
            }
            var request = new Sdk.Photon.Requests.SlotChangeType { SlotType = type, SlotId = slot };
            var data = BinarySerializer.Serialize<SlotChangeType>(request);
            PhotonNetwork.RaiseEvent(SLOT_CHANGE_TYPE,
                                     data,
                                     new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient },
                                     SendOptions.SendReliable);
        }

        void SlotChangeTypeDo(EventData photonEvent)
        {
            var request = BinarySerializer.Deserialize<SlotChangeType>((byte[])photonEvent.CustomData);
            if (request == null)
            {
                Log.E("Can't change slot type. Incorrect request");
                return;
            }
            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            var key = SlotGetTypeKey(request.SlotId);
            var val = props.TryGetOrDefault(key);
            if (val == null)
            {
                Log.E($"Can't change slot type. Invalid slot index: {request.SlotId}");
                return;
            }

            props[key] = request.SlotType;
            key = SlotGetOwnerKey(request.SlotId);
            props[key] = NetworkController.InvalidId;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        public void SendRoomSlotChangeOwner(int slot, int owner)
        {
            var request = new Sdk.Photon.Requests.SlotChangeOwner { OwnerId = owner, SlotId = slot };
            var data = BinarySerializer.Serialize<SlotChangeOwner>(request);
            PhotonNetwork.RaiseEvent(SLOT_CHANGE_OWNER,
                                     data,
                                     new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient },
                                     SendOptions.SendReliable);
        }

        void SlotChangeOwnerDo(EventData photonEvent)
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Log.E("Can't change slot owner. Not in a room");
                return;
            }

            var request = BinarySerializer.Deserialize<SlotChangeOwner>((byte[])photonEvent.CustomData);
            if (request == null)
            {
                Log.E("Can't change slot owner. Incorrect request");
                return;
            }

            if (!Controller.CurrentRoom.Players.Any(p => p.Id == request.OwnerId))
            {
                Log.E($"Can't change slot owner. No player in the room with id: {request.OwnerId}");
                return;
            }

            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            var key = SlotGetTypeKey(request.SlotId);
            var val = props.TryGetOrDefault(key);
            if (val == null)
            {
                Log.E($"Can't change slot owner. Invalid slot index: {request.SlotId}");
                return;
            }

            if ((int)val != SquadControllerData.TypeIndex(SquadControllerType.Player))
            {
                Log.E("Can't change slot owner. Can't insert player in NOT a player slot");
                return;
            }

            key = SlotGetOwnerKey(request.SlotId);
            val = props.TryGetOrDefault(key);
            if (val != null && (int)val != NetworkController.InvalidId)
            {
                Log.E("Can't change slot owner. Can't insert player in Reserved slot");
                return;
            }

            var slots = Controller.CurrentRoom.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var k = SlotGetOwnerKey(i);
                val = props.TryGetOrDefault(k);
                if (val != null && (int)val == request.OwnerId)
                {
                    props[k] = NetworkController.InvalidId;
                }
            }

            props[key] = request.OwnerId;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        public void SendGameStart()
        {
            if (!IsMaster())
            {
                return;
            }

            if (PhotonNetwork.CurrentRoom == null)
            {
                Log.E("Can't start game. Outside of the room");
                return;
            }

            var props = PhotonNetwork.CurrentRoom.CustomProperties;
            var levelObj = props.TryGetOrDefault(LEVEL_KEY);
            if (levelObj == null)
            {
                Log.E("Can't start game. Invalid level data");
                return;
            }

            var levels = S.Levels.Data;
            var levelIndex = (int)levelObj;
            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                Log.E("Can't start game. LevelIndex out of level range");
                return;
            }

            var isReady = true;
            var slots = Controller.CurrentRoom.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                var type = -1;
                var key = SlotGetTypeKey(i);
                var val = props.TryGetOrDefault(key);
                if (val != null)
                {
                    type = (int)val;
                }
                if (type == SquadControllerData.TypeIndex(SquadControllerType.Player))
                {
                    INetworkPlayer player = null;
                    var id = NetworkController.InvalidId;
                    key = SlotGetOwnerKey(i);
                    var ownerObject = props.TryGetOrDefault(key);
                    if (ownerObject != null)
                    {
                        id = (int)ownerObject;
                    }
                    player = Controller.CurrentRoom.Players.FirstOrDefault(p => p.Id == id);
                    if (player == null)
                    {
                        isReady = false;
                    }
                    else if (player != null)
                    {
                        key = PlayerReadyKey(player.Id);
                        var v = props.TryGetOrDefault(key);
                        {
                            isReady &= (bool)v;
                        }
                    }
                }
            }

            if (!isReady)
            {
                Log.E("Can't start game. Not all players are ready");
                return;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.RaiseEvent(GAME_START, null, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public void SendBattleStart()
        {
            if (!IsMaster())
            {
                return;
            }
            Log.I($"Send {nameof(TURN_START)} event");
            PhotonNetwork.RaiseEvent(TURN_START, PhotonNetwork.Time, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        void GameStartDo()
        {
            Controller.LoadLevel();
        }

        public void SendTurnFinishCompleted(int turn)
        {
            Log.I($"Send message {nameof(TURN_FINISH_COMPLETED)}.");
            PhotonNetwork.RaiseEvent(TURN_FINISH_COMPLETED, turn, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
        }

        public void SendTurnFinishLaunch(int turn)
        {
            Log.I($"Send message {nameof(TURN_FINISH_LAUNCH)}. Turn: {turn}");
            PhotonNetwork.RaiseEvent(TURN_FINISH_LAUNCH, turn, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        public void SendBattleAction(byte[] actionSerialized)
        {
            PhotonNetwork.RaiseEvent(ACTION, actionSerialized, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        }

        void ServerSave()
        {
            PlayerProfile.SaveString(SERVER_KEY, ServerDefaultInternal != null ? ServerDefaultInternal.Code : string.Empty);
        }

        public void Error(string message)
        {
            Log.E(message);
            if (IsMaster())
            {
                Log.E("Will leave room because error can't be fixed");
                RoomLeave();
            }
            else
            {
                PhotonNetwork.RaiseEvent(ERROR, message, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
            }
        }
    }
}
#endif