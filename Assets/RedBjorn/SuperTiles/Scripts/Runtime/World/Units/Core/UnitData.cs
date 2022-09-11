using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using RedBjorn.Utils;
using System;
using System.IO;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Unit informational storage
    /// </summary>
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Unit.Asset)]
    public class UnitData : ScriptableObjectExtended
    {
        [Serializable]
        public class StatData
        {
            public UnitStatTag Stat;
            public float Value;
        }

        public Sprite Avatar;
        public GameObject Model;
        public Vector3 UiHolder;
        public List<StatData> Stats;
        public List<ItemData> DefaultItems;

        public static UnitData FindDefault()
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(UnitData).Name, " ", "Default"));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
                return data;
            }

            guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(UnitData).Name));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
                return data;
            }
#endif
            return null;
        }

#if UNITY_EDITOR
        public static UnitData Create(string folderPath, string name, List<UnitData.StatData> stats)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var unit = ScriptableObject.CreateInstance<UnitData>();
                unit.Stats = stats;
                var unitPathUnique = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, string.Concat(name, FileFormat.Asset)));

                AssetDatabase.CreateAsset(unit, unitPathUnique);
                EditorUtility.SetDirty(unit);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Log.I($"New unit was created at path: {unitPathUnique}");

                return unit;
            }
            catch (Exception e)
            {
                Log.E(e);
                return null;
            }
        }
#endif
    }
}
