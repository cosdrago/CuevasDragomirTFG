using RedBjorn.SuperTiles.Items;
using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Item informational storage 
    /// </summary>
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Item.Asset)]
    public class ItemData : ScriptableObjectExtended
    {
        [Serializable]
        public class StatData
        {
            public ItemStatTag Stat;
            public float Value;
        }

        [Serializable]
        public class VisualConfig
        {
            public GameObject Model;
            [Header("UI")]
            public Sprite IconSmall;
            public bool UseColor;
            public Color Color;
            [Header("Trajectory")]
            public Material TrajectoryMaterial;
            [Header("Selector")]
            public bool UseSelectorCustom;
            public GameObject SelectorCustom;
            public ProtoTiles.MapSettings.TileVisual SelectorGenerated;
            [Header("Available")]
            public bool UseAvailableCustom;
            public GameObject AvailableCustom;
            public ProtoTiles.MapSettings.TileVisual AvailableGenerated;
            [Header("Obsolete")]
            public Material SelectorMaterial;
        }

        public string Caption;
        public bool Stackable;
        public int MaxStackCount = 1;

        [Space]
        public List<StatData> Stats = new List<StatData>();
        public List<ItemTag> Tags = new List<ItemTag>();
        public TargetSelector Selector;
        public ActionHandler ActionHandler;
        public VisualConfig Visual;
    }
}
