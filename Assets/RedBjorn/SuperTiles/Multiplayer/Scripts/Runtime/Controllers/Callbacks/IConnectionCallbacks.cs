namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which represents callback when network is connected/disconnected
    /// </summary>
    public interface IConnectionCallbacks
    {
        void OnConnected();
        void OnDisconnected();
    }
}
