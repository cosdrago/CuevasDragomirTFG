namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Entity for slot business logic inside a Room 
    /// </summary>
    public class RoomSlotEntity
    {
        public TeamTag Team;
        public SquadControllerType Type;
        public INetworkPlayer Player;

        RoomEntity Room;

        public RoomSlotEntity(TeamTag team, SquadControllerType type, RoomEntity room)
        {
            Team = team;
            Type = type;
            Room = room;
        }

        public bool IsReady()
        {
            return Room.Ready.Contains(Player);
        }
    }
}
