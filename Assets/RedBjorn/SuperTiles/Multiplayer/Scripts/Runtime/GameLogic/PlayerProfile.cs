using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer
{
    /// <summary>
    /// Class which encapsulates profile data
    /// </summary>
    public class PlayerProfile
    {
        static string NicknameShort;
        static string Suffix;

        public static string Nickname => Settings.AddDigits ? string.Concat(NicknameShort, Settings.Splitter, Suffix) : NicknameShort;
        static NetworkSettings.ProfileSettings Settings => S.Network.Profile;

        public static bool IsValid()
        {
            if (string.IsNullOrEmpty(NicknameShort))
            {
                NicknameLoad();
            }
            return !string.IsNullOrEmpty(NicknameShort);
        }

        public static string PreNickname()
        {
            return string.IsNullOrEmpty(NicknameShort) ? System.Environment.UserName : NicknameShort;
        }

        public static void SetNickname(string nickname)
        {
            var valid = nickname;
            if (string.IsNullOrEmpty(valid))
            {
                valid = PreNickname();
            }
            NicknameShort = valid;
            NicknameSave();
        }

        public static void NicknameSave()
        {
            SaveString("PlayerNickname", Nickname);
        }

        public static void NicknameLoad()
        {
            var nickname = LoadString("PlayerNickname");
            if (!string.IsNullOrEmpty(nickname))
            {
                var splits = nickname.Split(Settings.Splitter);
                NicknameShort = splits[0];
                if (splits.Length > 1)
                {
                    Suffix = splits[1];
                }
            }
            if (string.IsNullOrEmpty(Suffix))
            {
                Suffix = UnityEngine.Random.Range(0, 10000).ToString("0000");
            }
        }

        public static void SaveString(string key, string val)
        {
            PlayerPrefs.SetString(key, val);
        }

        public static string LoadString(string key)
        {
            return PlayerPrefs.GetString(key);
        }
    }
}
