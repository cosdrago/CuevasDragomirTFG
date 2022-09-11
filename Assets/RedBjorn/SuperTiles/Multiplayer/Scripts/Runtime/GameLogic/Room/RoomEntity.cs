using System.Collections.Generic;
using System.Linq;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Entity for room business logic
    /// </summary>
    public class RoomEntity : IComparer<INetworkPlayer>
    {
        public string Name;
        public int LevelIndex;
        public LevelData Level;
        public float TurnDuration;
        public List<RoomSlotEntity> Slots = new List<RoomSlotEntity>();
        public List<INetworkPlayer> Players = new List<INetworkPlayer>();
        public HashSet<INetworkPlayer> Ready = new HashSet<INetworkPlayer>();
        public HashSet<INetworkPlayer> Loaded = new HashSet<INetworkPlayer>();

        public RoomEntity(string name, int levelIndex, LevelData level, float turnDuration)
        {
            Name = name;
            LevelIndex = levelIndex;
            Level = level;
            TurnDuration = turnDuration;
            if (Level != null)
            {
                foreach (var slot in Level.Players)
                {
                    Slots.Add(new RoomSlotEntity(slot.Team, slot.ControlledBy, this));
                }
            }
            else
            {
                Log.E("Room data is invalid. No Level");
            }
        }

        public bool IsLoaded()
        {
            return Loaded.Count == Players.Count;
        }

        public void PlayerAdd(INetworkPlayer player)
        {
            Players.Add(player);
            Players.Sort(this);
        }

        public void PlayerRemove(int id)
        {
            var player = Players.FirstOrDefault(p => p.Id == id);
            PlayerRemove(player);
        }

        public void PlayerRemove(INetworkPlayer player)
        {
            if (player != null)
            {
                Players.Remove(player);
                Ready.Remove(player);
                Loaded.Remove(player);
            }
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Slots");
            foreach (var slot in Slots)
            {
                var playername = slot.Player == null ? "None" : $"{slot.Player.ToString()}";
                sb.AppendLine($"{slot.Team.name} - {slot.Type} - {playername}");
            }
            sb.AppendLine("\nPlayers");
            foreach (var player in Players)
            {
                sb.AppendLine(player.ToString());
            }
            sb.AppendLine("\nReady");
            foreach (var player in Ready)
            {
                sb.AppendLine(player.ToString());
            }
            return sb.ToString();
        }

        int IComparer<INetworkPlayer>.Compare(INetworkPlayer x, INetworkPlayer y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
            }

            return x.Id.CompareTo(y.Id);
        }
    }
}
