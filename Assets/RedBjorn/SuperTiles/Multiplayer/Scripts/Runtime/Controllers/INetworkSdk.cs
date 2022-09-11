namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// The core SDK interface which helps to communicate with network SDK
    /// </summary>
    public interface INetworkSdk
    {
        void Init(NetworkController controller);
        void Destroy();
        void Connect();
        void Disconnect();
        bool IsConnected { get; }

        string DebugInfo();
        double Time { get; }
        string ServerCurrent { get; }
        string ServerDefault { get; set; }
        string[] Servers { get; }
        int Id { get; }
        bool IsInRoom { get; }
        bool IsMaster(INetworkPlayer player);
        bool IsMaster();

        void LobbyJoin();
        void RoomCreate(RoomEntity room);
        void RoomJoin(string id);
        void RoomJoinRandom();
        void RoomLeave();

        void SendRoomSlotChangeType(int slot, int type);
        void SendRoomSlotChangeOwner(int slot, int owner);
        void SendIsReady();
        void SendGameStart();
        void SendBattleIsLoaded();
        void SendBattleStart();
        void SendBattleAction(byte[] actionSerialized);
        void SendTurnFinishLaunch(int turn);
        void SendTurnFinishCompleted(int turn);
        void Error(string message);
    }
}
