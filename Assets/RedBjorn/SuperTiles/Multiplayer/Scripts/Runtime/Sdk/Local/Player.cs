namespace RedBjorn.SuperTiles.Multiplayer.Sdk.Local
{
    /// <summary>
    /// Local player
    /// </summary>
    public class Player : INetworkPlayer
    {
        public int Id => 0;
        public string Nickname => PlayerProfile.Nickname;
        public bool IsMasterClient => true;

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
