namespace RedBjorn.SuperTiles.Multiplayer.Sdk.Photon.Loggers
{
    /// <summary>
    /// Separate logger for PhotonSDK
    /// </summary>
    public class Photon : SuperTiles.Logger
    {
        public override bool IsEnabled => true;
    }
}
