namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which represents network callbacks when player is inside a lobby
    /// </summary>
    public interface ILobbyRoomCallbacks
    {
        void OnRoomListUpdate();
    }
}
