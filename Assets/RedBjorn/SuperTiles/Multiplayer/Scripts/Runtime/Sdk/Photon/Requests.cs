#if PHOTON_UNITY_NETWORKING
namespace RedBjorn.SuperTiles.Multiplayer.Sdk.Photon.Requests
{
    /// <summary>
    /// Photon request info for changing slot owner
    /// </summary>
    [System.Serializable]
    public class SlotChangeOwner
    {
        public int SlotId;
        public int OwnerId;
    }

    /// <summary>
    /// Photon request info for changing slot type
    /// </summary>
    [System.Serializable]
    public class SlotChangeType
    {
        public int SlotId;
        public int SlotType;
    }
}
#endif