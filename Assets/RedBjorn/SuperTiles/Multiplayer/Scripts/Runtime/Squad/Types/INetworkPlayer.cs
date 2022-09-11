namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which represents network player
    /// </summary>
    public interface INetworkPlayer
    {
        int Id { get; }
        string Nickname { get; }
        bool IsMasterClient { get; }
    }
}
