namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Interface which handles network callbacks about battle actions
    /// </summary>
    public interface IOnBattleActionCallbacks
    {
        void OnBattleAction(byte[] serializedAction);
    }
}