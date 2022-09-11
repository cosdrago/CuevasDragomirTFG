using RedBjorn.SuperTiles.Multiplayer.Sdk;
using System.Collections.Generic;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Controller which represents all network communication
    /// </summary>
    public class NetworkController
    {
        /// <summary>
        /// Local player
        /// </summary>
        public INetworkPlayer Player;

        /// <summary>
        /// Information about current room the local player is in
        /// </summary>
        public RoomEntity CurrentRoom;

        /// <summary>
        /// The list of room information that could be seen from a lobby
        /// </summary>
        public List<RoomLobbyEntity> LobbyRooms = new List<RoomLobbyEntity>();

        /// <summary>
        /// Network connect/disconnect callback list 
        /// </summary>
        public List<IConnectionCallbacks> ConnectionTargets = new List<IConnectionCallbacks>();

        /// <summary>
        /// Callback list which reacts on a room network status changes
        /// </summary>
        public List<IRoomCallbacks> RoomTargets = new List<IRoomCallbacks>();

        /// <summary>
        /// Callback list which reacts on the events when the local player is inside room
        /// </summary>
        public List<IInRoomCallbacks> InRoomTargets = new List<IInRoomCallbacks>();

        /// <summary>
        /// Callback list which reacts on turn events
        /// </summary>
        public List<ITurnCallbacks> TurnTargets = new List<ITurnCallbacks>();

        /// <summary>
        /// Callback list which reacts on battle action events
        /// </summary>
        public List<IOnBattleActionCallbacks> BattleActionTargets = new List<IOnBattleActionCallbacks>();

        /// <summary>
        /// Callback list which reacts on lobby events
        /// </summary>
        public List<ILobbyRoomCallbacks> LobbyRoomTargets = new List<ILobbyRoomCallbacks>();

        /// <summary>
        /// Current SDK
        /// </summary>
        INetworkSdk Sdk;

        /// <summary>
        /// ID for in
        /// </summary>
        public const int InvalidId = -1;

        /// <summary>
        /// Network time now
        /// </summary>
        public static double Time => Instance.Sdk.Time;

        /// <summary>
        /// Check if the controller is connected to the network
        /// </summary>
        /// <returns></returns>
        public static bool IsConnected => Instance.Sdk.IsConnected;

        /// <summary>
        /// Check if the local player is in a room
        /// </summary>
        /// <returns></returns>
        public static bool IsInRoom => Instance.Sdk.IsInRoom;

        /// <summary>
        /// Get the local player
        /// </summary>
        /// <returns></returns>
        public static INetworkPlayer LocalPlayer => Instance.Player;

        /// <summary>
        /// Get the id of local player
        /// </summary>
        /// <returns></returns>
        public static int Id => Instance.Sdk.Id;

        /// <summary>
        /// Get an array of server names
        /// </summary>
        /// <returns></returns>
        public static string[] Servers => Instance.Sdk.Servers;

        public NetworkController()
        {
            Sdk = S.Network.SdkCreate();
            if (Sdk == null)
            {
                Log.W($"No sdk selected. {nameof(LocalSdk)} will be selected");
                Sdk = new LocalSdk();
            }
            Sdk.Init(this);
            var go = new UnityEngine.GameObject(nameof(NetworkController));
            go.transform.SetPositionAndRotation(UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
            go.AddComponent<NetworkControllerView>();
        }

        static NetworkController CachedInstance;
        static NetworkController Instance
        {
            get
            {
                if (CachedInstance == null)
                {
                    CachedInstance = new NetworkController();
                    Log.I($"{nameof(NetworkController)} created");
                }
                return CachedInstance;
            }
        }

        /// <summary>
        /// Network server default name
        /// </summary>
        public static string ServerDefault
        {
            get
            {
                return Instance.Sdk.ServerDefault;
            }
            set
            {
                Instance.Sdk.ServerDefault = value;
            }
        }

        /// <summary>
        /// Network server current name
        /// </summary>
        public static string ServerCurrent
        {
            get
            {
                return Instance.Sdk.ServerCurrent;
            }
        }

        /// <summary>
        /// Destroy controller
        /// </summary>
        public static void Destroy()
        {
            if (CachedInstance != null)
            {
                CachedInstance.DestroyInternal();
            }
            else
            {
                Log.W($"Can't destroy {nameof(NetworkController)}. Wasn't created");
            }
        }

        /// <summary>
        /// Connect controller to the network
        /// </summary>
        public static void Connect()
        {
            Instance.Sdk.Connect();
        }

        /// <summary>
        /// Disconnect controller from the network
        /// </summary>
        public static void Disconnect()
        {
            Instance.DisconnectInternal();
        }

        /// <summary>
        /// Get debug network information
        /// </summary>
        /// <returns></returns>
        public static string DebugInfo()
        {
            return Instance.Sdk.DebugInfo();
        }

        /// <summary>
        /// Check if the local player is a master client
        /// </summary>
        /// <returns></returns>
        public static bool IsMaster()
        {
            return Instance.Sdk.IsMaster();
        }

        /// <summary>
        /// Check if a player is a master client
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsMaster(INetworkPlayer player)
        {
            return Instance.Sdk.IsMaster(player);
        }

        /// <summary>
        /// Get room information the local player is in
        /// </summary>
        /// <returns></returns>
        public static RoomEntity GetCurrentRoom()
        {
            return Instance.CurrentRoom;
        }

        /// <summary>
        /// Get the list of rooms when the local player is in a lobby
        /// </summary>
        /// <returns></returns>
        public static List<RoomLobbyEntity> GetLobbyRooms()
        {
            return Instance.LobbyRooms;
        }

        /// <summary>
        /// Join default lobby
        /// </summary>
        public static void LobbyJoin()
        {
            Instance.Sdk.LobbyJoin();
        }

        /// <summary>
        /// Create room
        /// </summary>
        /// <param name="room"></param>
        public static void RoomCreate(RoomEntity room)
        {
            Instance.Sdk.RoomCreate(room);
        }

        /// <summary>
        /// Join room
        /// </summary>
        /// <param name="id">specific room id</param>
        public static void RoomJoin(string id)
        {
            Instance.Sdk.RoomJoin(id);
        }

        /// <summary>
        /// Join random room
        /// </summary>
        public static void RoomJoinRandom()
        {
            Instance.Sdk.RoomJoinRandom();
        }

        /// <summary>
        /// Leave current room
        /// </summary>
        public static void RoomLeave()
        {
            Instance.Sdk.RoomLeave();
        }

        /// <summary>
        /// Get a player who is in a slot
        /// </summary>
        /// <param name="slot">slot index</param>
        /// <returns></returns>
        public static INetworkPlayer RoomSlotOwner(int slot)
        {
            return Instance.RoomSlotOwnerInternal(slot);
        }

        /// <summary>
        /// Send network message when the local player is ready or not
        /// </summary>
        public static void SendIsReady()
        {
            Instance.Sdk.SendIsReady();
        }

        /// <summary>
        /// Send network message when the local player have loaded selected level
        /// </summary>
        public static void SendBattleIsLoaded()
        {
            Instance.Sdk.SendBattleIsLoaded();
        }

        /// <summary>
        /// Send network message to change slot type
        /// </summary>
        /// <param name="slot">slot index</param>
        /// <param name="type">type index</param>
        public static void SendRoomSlotChangeType(int slot, int type)
        {
            Instance.Sdk.SendRoomSlotChangeType(slot, type);
        }

        /// <summary>
        /// Send network message to change slot owner
        /// </summary>
        /// <param name="slot">slot index</param>
        /// <param name="owner">player index</param>
        public static void SendRoomSlotChangeOwner(int slot, int owner)
        {
            Instance.Sdk.SendRoomSlotChangeOwner(slot, owner);
        }

        /// <summary>
        /// Send network message to start level loading
        /// </summary>
        public static void SendGameStart()
        {
            Instance.Sdk.SendGameStart();
        }

        /// <summary>
        /// Send network message to the battle
        /// </summary>
        public static void SendBattleStart()
        {
            Instance.Sdk.SendBattleStart();
        }

        /// <summary>
        /// Send network message to launch turn finish procecs
        /// </summary>
        /// <param name="turn">turn number</param>
        public static void SendTurnFinishLaunch(int turn)
        {
            Instance.Sdk.SendTurnFinishLaunch(turn);
        }

        /// <summary>
        /// Send network message when the local player have finished the turn
        /// </summary>
        /// <param name="turn">turn number</param>
        public static void SendTurnFinishCompleted(int turn)
        {
            Instance.Sdk.SendTurnFinishCompleted(turn);
        }

        /// <summary>
        /// Send network message to play battle action
        /// </summary>
        /// <param name="actionSerialized">action serialized to byte array</param>
        public static void SendBattleAction(byte[] actionSerialized)
        {
            Instance.Sdk.SendBattleAction(actionSerialized);
        }

        /// <summary>
        /// Send error message over the network
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Instance.Sdk.Error(message);
        }

        /// <summary>
        /// Add callback to the network events
        /// </summary>
        /// <param name="target"></param>
        public static void AddCallbackTarget(object target)
        {
            if (target is IRoomCallbacks room)
            {
                Instance.RoomTargets.Add(room);
            }
            if (target is IConnectionCallbacks connection)
            {
                Instance.ConnectionTargets.Add(connection);
            }
            if (target is IInRoomCallbacks inRoom)
            {
                Instance.InRoomTargets.Add(inRoom);
            }
            if (target is ITurnCallbacks turn)
            {
                Instance.TurnTargets.Add(turn);
            }
            if (target is IOnBattleActionCallbacks action)
            {
                Instance.BattleActionTargets.Add(action);
            }
            if (target is ILobbyRoomCallbacks lobbyRoom)
            {
                Instance.LobbyRoomTargets.Add(lobbyRoom);
            }
        }

        /// <summary>
        /// Remove callback to the network events
        /// </summary>
        public static void RemoveCallbackTarget(object target)
        {
            if (target is IRoomCallbacks room)
            {
                Instance.RoomTargets.Remove(room);
            }
            if (target is IConnectionCallbacks connection)
            {
                Instance.ConnectionTargets.Remove(connection);
            }
            if (target is IInRoomCallbacks inRoom)
            {
                Instance.InRoomTargets.Remove(inRoom);
            }
            if (target is ITurnCallbacks turn)
            {
                Instance.TurnTargets.Remove(turn);
            }
            if (target is IOnBattleActionCallbacks action)
            {
                Instance.BattleActionTargets.Remove(action);
            }
            if (target is ILobbyRoomCallbacks lobbyRoom)
            {
                Instance.LobbyRoomTargets.Remove(lobbyRoom);
            }
        }

        /// <summary>
        /// Load level from current room information
        /// </summary>
        public void LoadLevel()
        {
            if (GameEntity.Current != null)
            {
                Log.E("Won't load level. There is an existed GameEntity state");
                return;
            }
            var level = CurrentRoom.Level;
            GameEntity.Current = new GameEntity
            {
                Creator = new GameTypeCreators.Multiplayer(),
                Restartable = false,
                Level = level
            };
            SceneLoader.Load(level.SceneName, S.Levels.GameSceneName);
        }

        /// <summary>
        /// Internal destroy logic
        /// </summary>
        void DestroyInternal()
        {
            Sdk.Destroy();
            Sdk = null;
            Log.I($"{nameof(NetworkController)} destroyed");
        }

        /// <summary>
        /// Internal disconnect logic
        /// </summary>
        void DisconnectInternal()
        {
            Instance.Sdk.Disconnect();
            Player = null;
            CurrentRoom = null;
            LobbyRooms.Clear();
        }

        /// <summary>
        /// Internal change room slot owner logic
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        INetworkPlayer RoomSlotOwnerInternal(int slot)
        {
            if (CurrentRoom == null)
            {
                return null;
            }

            if (slot < 0 || slot >= Instance.CurrentRoom.Slots.Count)
            {
                return null;
            }

            return Instance.CurrentRoom.Slots[slot].Player;
        }
    }
}
