using RedBjorn.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors.Setup
{
    public class SetupWindowSettings : ScriptableObjectExtended
    {
        public List<SceneAsset> Scenes = new List<SceneAsset>();

        public static string SetupKeyOld => string.Concat(nameof(RedBjorn), ".", nameof(RedBjorn.SuperTiles), ".", nameof(Setup), "-", Application.dataPath);
        public static string SetupKey => string.Concat(nameof(RedBjorn), ".", nameof(RedBjorn.SuperTiles), "_", Application.dataPath, "-", nameof(Setup));
        public static string VersionKey => string.Concat(nameof(RedBjorn), ".", nameof(RedBjorn.SuperTiles), "-", Application.dataPath, "-", "Version");
        public static string SetupMultiplayerKey => string.Concat(nameof(RedBjorn), ".", nameof(RedBjorn.SuperTiles), "_", Application.dataPath, "-", nameof(Setup), "_Multiplayer");
        public static string MultiplayerVersionKey => string.Concat(nameof(RedBjorn), ".", nameof(RedBjorn.SuperTiles), "-", Application.dataPath, "-Multiplayer_Version");

        public void AddToBuildSettings()
        {
            var guidsExisted = EditorBuildSettings.scenes.Select(s => s.guid.ToString());
            var guidsNew = Scenes.Select(s => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s)));
            EditorBuildSettings.scenes = guidsNew.Union(guidsExisted).Select(s => new EditorBuildSettingsScene(new GUID(s), true)).ToArray();
        }

        public static void AddScenesToBuildSettings()
        {
            if (!EditorPrefs.GetBool(SetupMultiplayerKey))
            {
                var instance = Instance();
                if (instance)
                {
                    instance.AddToBuildSettings();
                    EditorPrefs.SetBool(SetupMultiplayerKey, true);
                }
                else
                {
                    Log.E($"Can't do proper {nameof(RedBjorn.SuperTiles)} setup");
                }
            }
            else
            {
                Log.E("Already setup");
            }
        }

        public static void ItemsUpdate()
        {
            var items = AssetDatabaseUtils.FindAssets<ItemData>();
            foreach (var item in items)
            {
                if (item.Visual == null)
                {
                    item.Visual = new ItemData.VisualConfig();
                }

                var existedSelector = item.Visual.SelectorMaterial;
                if (item.Visual.SelectorGenerated == null)
                {
                    item.Visual.SelectorGenerated = new ProtoTiles.MapSettings.TileVisual();
                }
                item.Visual.SelectorGenerated.BorderSize = 0.1f;
                if (existedSelector)
                {
                    item.Visual.SelectorGenerated.ShowInner = true;
                    item.Visual.SelectorGenerated.Inner = existedSelector;
                    item.Visual.SelectorGenerated.ShowBorder = false;
                    item.Visual.SelectorGenerated.Border = existedSelector;
                }

                var availableDefault = Item.ItemWindowSettings.Instance.AvailableDefault;
                if (item.Visual.AvailableGenerated == null)
                {
                    item.Visual.AvailableGenerated = new ProtoTiles.MapSettings.TileVisual();
                }
                item.Visual.AvailableGenerated.BorderSize = 0.25f;
                item.Visual.AvailableGenerated.ShowInner = true;
                item.Visual.AvailableGenerated.Inner = availableDefault;
                item.Visual.AvailableGenerated.ShowBorder = false;
                item.Visual.AvailableGenerated.Border = null;

                var trajectory = Item.ItemWindowSettings.Instance.TrajectoryDefault;
                item.Visual.TrajectoryMaterial = trajectory;

                item.Visual.SelectorMaterial = null;
                EditorUtility.SetDirty(item);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BattleFinishCreate()
        {
            var levels = LevelData.FindAll().Where(l => !l.BattleFinish);
            foreach (var level in levels)
            {
                var path = AssetDatabase.GetAssetPath(level);
                var folder = Path.GetDirectoryName(path);
                var target = string.Concat("_", LevelData.Suffix);
                var name = Path.GetFileNameWithoutExtension(path).Replace(target, string.Empty);
                var handler = BattleFinishHandler.Create(folder, name, nameof(BattleFinish.OneSquadLeft));
                level.BattleFinish = handler;
                EditorUtility.SetDirty(level);
            }
            if (levels.Any())
            {
                AssetDatabase.Refresh();
            }
        }

        public static string GetVersion()
        {
            var path = AssetDatabase.GetAssetPath(Instance());
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            var rootfolder = Path.Combine(Path.GetDirectoryName(path), "..", "..");
            var versionName = "Version.txt";
            var versionPath = Path.Combine(rootfolder, versionName);
            string version = null;
            try
            {
                version = File.ReadAllText(versionPath);
            }
            catch (Exception e)
            {
                Log.E($"Coudn't get current version. Cause {e.Message}");
                version = null;
            }
            return version;
        }

        static SetupWindowSettings Instance()
        {
            var path = AssetDatabase.FindAssets("t:" + nameof(SetupWindowSettings))
                         .Select(a => AssetDatabase.GUIDToAssetPath(a))
                         .OrderBy(a => a)
                         .FirstOrDefault();
            return AssetDatabase.LoadAssetAtPath<SetupWindowSettings>(path);
        }
    }
}
