using RedBjorn.SuperTiles.Multiplayer.Sdk.Photon;
using System.Collections.Generic;

namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Class of PhotonSDK 
    /// </summary>
    public class PhotonSdkCreator : NetworkSdkCreator
    {
        public List<Server> Servers = new List<Server>();

        public override INetworkSdk Create()
        {
            INetworkSdk Sdk = null;
#if PHOTON_UNITY_NETWORKING
            Sdk = new PhotonSdk(Servers);
#endif
            return Sdk;
        }
    }
}
