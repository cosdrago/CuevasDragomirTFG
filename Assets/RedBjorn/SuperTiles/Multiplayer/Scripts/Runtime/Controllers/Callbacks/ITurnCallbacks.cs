namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which handles network turn callbacks
    /// </summary>
    public interface ITurnCallbacks
    {
        void OnTurnStart(double timeStart);
        void OnTurnFinishLaunched(int turn);
        void OnTurnFinishCompleted(INetworkPlayer player, int turn);
    }
}
