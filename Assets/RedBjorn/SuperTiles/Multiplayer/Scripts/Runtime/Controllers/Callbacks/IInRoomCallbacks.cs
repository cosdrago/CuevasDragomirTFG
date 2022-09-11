namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which represents network callbacks when player is inside a room
    /// </summary>
    public interface IInRoomCallbacks
    {
        void OnPlayerEnteredRoom();
        void OnPlayerLeftRoom(INetworkPlayer player);
        void OnRoomPropertiesUpdate();
        void OnPlayerPropertiesUpdate();
        void OnMasterClientSwitched();
    }
}
