#if PHOTON_UNITY_NETWORKING
namespace RedBjorn.SuperTiles.Multiplayer.Sdk.Photon
{
    /// <summary>
    /// Photon player
    /// </summary>
    public class Player : INetworkPlayer
    {
        public global::Photon.Realtime.Player PlayerInternal;

        public int Id => PlayerInternal.ActorNumber;
        public bool IsMasterClient => PlayerInternal.IsMasterClient;
        public string Nickname => PlayerInternal.NickName;

        public Player(global::Photon.Realtime.Player player)
        {
            PlayerInternal = player;
        }

        public override string ToString()
        {
            if (IsMasterClient)
            {
                return $"{Nickname} (id: {Id}) (master)";
            }
            return $"{Nickname} (id: {Id})";
        }
    }
}
#endif