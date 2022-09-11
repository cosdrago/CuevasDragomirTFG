namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which handles network room callbacks
    /// </summary>
    public interface IRoomCallbacks
    {
        void OnJoined();
        void OnJoinedFailed();
        void OnLeft();
        void OnCreated();
        void OnCreateFailed();
    }
}
