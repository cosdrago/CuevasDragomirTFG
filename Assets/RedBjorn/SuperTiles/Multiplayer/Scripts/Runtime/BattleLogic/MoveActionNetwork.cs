using RedBjorn.Utils;
using System;
using System.Linq;

namespace RedBjorn.SuperTiles.Battle.Actions
{
    public partial class MoveAction : BaseAction
    {
        /// <summary>
        /// Class which represents MoveAction that could be sent over network
        /// </summary>
        [Serializable]
        public class Network
        {
            public int Player;
            public int Unit;
            public SerializableVector3 Position;
        }

        public MoveAction(byte[] serializedAction, BattleEntity battle)
        {
            var network = BinarySerializer.Deserialize<Network>(serializedAction);
            if (network.Player >= 0 && network.Player < battle.Players.Count)
            {
                PlayerInternal = battle.Players[network.Player];
            }
            UnitInternal = battle.UnitsAlive.FirstOrDefault(u => u.Id == network.Unit);
            Point = network.Position;
        }

        public override byte[] Serialize()
        {
            var plain = new Network
            {
                Player = UnitInternal.Game.Battle.Players.FindIndex(p => p == Player),
                Unit = UnitInternal.Id,
                Position = Point
            };
            var serializedAction = BinarySerializer.Serialize(plain);
            var message = new byte[serializedAction.Length + 1];
            message[0] = MoveType;
            serializedAction.CopyTo(message, 1);
            return message;
        }
    }
}