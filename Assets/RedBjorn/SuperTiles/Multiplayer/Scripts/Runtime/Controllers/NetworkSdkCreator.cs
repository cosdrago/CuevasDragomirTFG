using RedBjorn.Utils;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Base class for creation network SDK
    /// </summary>
    public abstract class NetworkSdkCreator : ScriptableObjectExtended
    {
        public abstract INetworkSdk Create();
    }
}
