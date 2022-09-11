using RedBjorn.Utils;
using System;
using System.Linq;

namespace RedBjorn.SuperTiles.Battle.Actions
{
    public partial class ItemAction : BaseAction
    {
        /// <summary>
        /// Class which represents ItemAction that could be sent over network
        /// </summary>
        [Serializable]
        public class Network
        {
            public int Player;
            public int Unit;
            public SerializableVector3 Position;
            public int Item;
        }

        public static byte Type = 1;

        public ItemAction(byte[] serializedAction, BattleEntity battle)
        {
            var network = BinarySerializer.Deserialize<Network>(serializedAction);
            if (network.Player >= 0 && network.Player < battle.Players.Count)
            {
                PlayerInternal = battle.Players[network.Player];
            }
            UnitInternal = battle.UnitsAlive.FirstOrDefault(u => u.Id == network.Unit);
            if (UnitInternal != null && network.Item >= 0 && network.Item < UnitInternal.Items.Count)
            {
                Item = UnitInternal.Items[network.Item];
            }
            Position = network.Position;
        }

        public override byte[] Serialize()
        {
            var plain = new Network
            {
                Player = Unit.Game.Battle.Players.FindIndex(p => p == PlayerInternal),
                Unit = Unit.Id,
                Item = Unit.Items.FindIndex(i => i == Item),
                Position = Position,
            };
            var serializedAction = BinarySerializer.Serialize(plain);
            var message = new byte[serializedAction.Length + 1];
            message[0] = Type;
            serializedAction.CopyTo(message, 1);
            return message;
        }
    }
}
