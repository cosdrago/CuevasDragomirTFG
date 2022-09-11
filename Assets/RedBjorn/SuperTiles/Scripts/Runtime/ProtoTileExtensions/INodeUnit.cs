namespace RedBjorn.SuperTiles.Map
{
    /// <summary>
    /// Node interface which extents Node logic to handle units
    /// </summary>
    public interface INodeUnit
    {
        UnitEntity Unit { get; }
        void RegisterUnit(UnitEntity unit);
        void UnregisterUnit();
    }
}
