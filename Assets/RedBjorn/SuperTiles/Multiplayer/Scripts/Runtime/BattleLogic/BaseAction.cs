namespace RedBjorn.SuperTiles.Battle
{
    /// <summary>
    /// Base class for unit action which declares Serialize/Deserialize logic
    /// </summary>
    public abstract partial class BaseAction
    {
        public static byte MoveType = 0;
        public static byte ItemType = 1;

        public abstract byte[] Serialize();

        public static BaseAction Deserialize(byte[] data, BattleEntity battle)
        {
            BaseAction action = null;
            if (data.Length > 0)
            {
                var serializedAction = new byte[data.Length - 1];
                for (int i = 0; i < serializedAction.Length; i++)
                {
                    serializedAction[i] = data[i + 1];
                }
                var type = data[0];
                if (type == MoveType)
                {
                    action = new Actions.MoveAction(serializedAction, battle);
                }
                else if (type == ItemType)
                {
                    action = new Actions.ItemAction(serializedAction, battle);
                }
                else
                {
                    Log.E("Can't create action. Unknown type");
                }
            }
            else
            {
                Log.E("Can't create action. Data message is empty");
            }
            return action;
        }
    }
}
