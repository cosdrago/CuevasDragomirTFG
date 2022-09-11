using RedBjorn.SuperTiles.Multiplayer;
using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Global network settings
    /// </summary>
    public class NetworkSettings : ScriptableObjectExtended
    {
        [Serializable]
        public class AiSettings
        {
            public List<string> Nicknames;
        }

        [Serializable]
        public class ProfileSettings
        {
            public bool AddDigits;
            public char Splitter = '#';
        }

        public List<NetworkSdkCreator> Sdks;
        public List<float> TurnDurations;
        public float TurnDurationVerifyDelay;
        public float TurnFinishCompletedVerifyDelay;
        public AiSettings Ai;
        public ProfileSettings Profile;

        public INetworkSdk SdkCreate()
        {
            var sdk = Sdks.FirstOrDefault(s => s);
            if (sdk == null)
            {
                Log.E("Can't create Network Sdk. There are no appropriate settings");
                return null;
            }

            return sdk.Create();
        }
    }
}

