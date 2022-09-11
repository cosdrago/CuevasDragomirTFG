namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Entity for business logic of room inside lobby
    /// </summary>
    public class RoomLobbyEntity
    {
        public string Id;
        public int PlayersCount;
        public int Level;
        public float TurnDuration;

        public string Description
        {
            get
            {
                var playersMax = -1;
                var levelname = "Unknown level";
                var levels = S.Levels.Data;
                if (Level >= 0 || Level < levels.Count)
                {
                    var level = levels[Level];
                    levelname = level.Caption;
                    playersMax = level.Players.Count;
                }
                return $"{levelname} ({TurnDuration.ToString("F0")} sec) - ({PlayersCount}/{playersMax})";
            }
        }
    }
}
