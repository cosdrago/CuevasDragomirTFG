using RedBjorn.Utils;

namespace RedBjorn.SuperTiles.Settings
{
    /// <summary>
    /// Root settings object
    /// </summary>
    public partial class GameSettings : ScriptableObjectExtended
    {
        public LevelSettings Levels;
        public InputSettings Input;
        public PrefabSettings Prefabs;
        public SoundSettings Sound;
        public BattleSettings Battle;
        public LogSettings Log;
    }
}