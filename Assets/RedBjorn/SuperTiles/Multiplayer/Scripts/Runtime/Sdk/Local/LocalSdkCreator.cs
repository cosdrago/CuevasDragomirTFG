namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Creator of LocalSDK
    /// </summary>
    public class LocalSdkCreator : NetworkSdkCreator
    {
        public override INetworkSdk Create()
        {
            return new LocalSdk();
        }
    }
}