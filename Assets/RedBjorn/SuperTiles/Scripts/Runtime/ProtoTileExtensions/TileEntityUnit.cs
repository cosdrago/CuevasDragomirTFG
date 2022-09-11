using RedBjorn.SuperTiles;
using RedBjorn.SuperTiles.Map;

namespace RedBjorn.ProtoTiles
{
    /// <summary>
    /// Extension of TileEntity for unit handling
    /// </summary>
    public partial class TileEntity : INodeUnit
    {
        public bool HasUnit { get { return Unit != null; } }

        UnitEntity UnitInternal;
        public UnitEntity Unit { get { return UnitInternal; } }

        public void RegisterUnit(UnitEntity unit)
        {
            UnitInternal = unit;
            if (UnitInternal != null)
            {
                ObstacleCount++;
            }
        }

        public void UnregisterUnit()
        {
            if (UnitInternal != null)
            {
                ObstacleCount--;
            }
            UnitInternal = null;
        }
    }
}

