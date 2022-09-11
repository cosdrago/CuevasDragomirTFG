#if PHOTON_UNITY_NETWORKING
namespace RedBjorn.SuperTiles.Multiplayer.Sdk
{
    /// <summary>
    /// Holder for Photon keys
    /// </summary>
    public partial class PhotonSdk
    {
        public class Log : RedBjorn.Utils.Log<Photon.Loggers.Photon> { }

        public const byte SLOT_CHANGE_TYPE = 1;
        public const byte SLOT_CHANGE_OWNER = 2;
        public const byte GAME_START = 3;
        public const byte ACTION = 4;
        public const byte TURN_FINISH_LAUNCH = 5;
        public const byte TURN_FINISH_COMPLETED = 6;
        public const byte TURN_START = 7;
        public const byte ERROR = 8;
        public const byte LEAVE = 9;

        public const string SLOT_TYPE_KEY = "ST";
        public const string SLOT_OWNER_KEY = "SO";
        public const string LEVEL_KEY = "LV";
        public const string TURN_DURATION_KEY = "TD";
        public const string PLAYERS_COUNT_KEY = "PC";
        public const string PLAYER_READY_KEY = "PR";
        public const string SESSION_LOADED_KEY = "SS";
        public const string SERVER_KEY = "PhotonServer";

        int SlotGetType(string value)
        {
            return Parse(value, SLOT_TYPE_KEY);
        }

        string SlotGetTypeKey(int slot)
        {
            return $"{SLOT_TYPE_KEY}{slot}";
        }

        int SlotGetOwner(string value)
        {
            return Parse(value, SLOT_OWNER_KEY);
        }

        string SlotGetOwnerKey(int slot)
        {
            return $"{SLOT_OWNER_KEY}{slot}";
        }

        int PlayerReadyGetPlayer(string value)
        {
            return Parse(value, PLAYER_READY_KEY);
        }

        string PlayerReadyKey(int id)
        {
            return $"{PLAYER_READY_KEY}{id}";
        }

        int SessionLoadedGetPlayer(string value)
        {
            return Parse(value, SESSION_LOADED_KEY);
        }

        string SessionLoadedKey(int id)
        {
            return $"{SESSION_LOADED_KEY}{id}";
        }

        int Parse(string value, string prefix)
        {
            string keyString = null;
            if (value.StartsWith(prefix))
            {
                keyString = value.Substring(prefix.Length);
            }
            var index = -1;
            if (!string.IsNullOrEmpty(keyString) && !int.TryParse(keyString, out index))
            {
                index = -1;
            }
            return index;
        }
    }
}
#endif