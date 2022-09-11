using System;
using System.Collections.Generic;
using System.IO;
using RedBjorn.ProtoTiles;
using RedBjorn.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Level informational storage
    /// </summary>
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Level.Asset)]
    public class LevelData : ScriptableObjectExtended
    {
        [Serializable]
        public class CameraData
        {
            public Vector3 StartPosition;
            public GameObject Prefab;
        }

        public string Caption;
        public string SceneName;
        [Tooltip("Should battle start after level load")]
        public bool AutoStart = true;
        [Tooltip("When same effect is added, should it only increase effect duration")]
        public bool EqualEffectInfluenceDuration = true;
        public MapSettings Map;
        public TurnResolver Turn;
        public ActionRules Actions;
        public HealthConvertRules Health;
        public BattleFinishHandler BattleFinish;
        public CameraData Camera;
        public List<SquadControllerData> Players;

        public static LevelData Find(string sceneName)
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(LevelData).Name));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (level && level.SceneName.Equals(sceneName))
                {
                    return level;
                }
            }
#endif
            return null;
        }

#if UNITY_EDITOR
        public const string Suffix = "Level";

        public static LevelData Create(string directory, string levelName, MapSettings map, TurnResolver turn, ActionRules actions, HealthConvertRules health)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var levelPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, string.Concat(levelName, "_", Suffix, FileFormat.Asset)));
                var level = ScriptableObject.CreateInstance<LevelData>();
                level.Caption = levelName;
                level.AutoStart = true;
                level.EqualEffectInfluenceDuration = true;
                level.Map = map;
                level.Turn = turn;
                level.Actions = actions;
                level.Health = health;
                level.Camera = new CameraData();
                level.Players = new List<SquadControllerData>
                {
                    new SquadControllerData
                    {
                        Name = "Player",
                        Team = TeamTag.Find(),
                        ControlledBy = SquadControllerType.Player
                    },
                    new SquadControllerData
                    {
                        Name = "Ai",
                        Team = TeamTag.Find(" 2"),
                        ControlledBy = SquadControllerType.AI
                    }
                };

                AssetDatabase.CreateAsset(level, levelPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Log.I($"New level was created at path: {levelPath}");
                return level;
            }
            catch (Exception e)
            {
                Log.E(e);
                return null;
            }
        }

        public static LevelData Duplicate(LevelData level, string folder, string filename)
        {
            LevelData newLevel = null;
            if (!level)
            {
                return newLevel;
            }
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var path = string.Format("{0}/{1}_{2}{3}", folder, filename, LevelData.Suffix, FileFormat.Asset);
                var levelPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (string.IsNullOrEmpty(levelPath))
                {
                    throw new Exception($"Could not genereate unique path for {path}");
                }

                string mapPath = null;
                if (level.Map)
                {
                    path = MapSettings.Path(folder, filename, level.Map.Type.ToString());
                    mapPath = AssetDatabase.GenerateUniqueAssetPath(path);
                    if (string.IsNullOrEmpty(mapPath))
                    {
                        throw new Exception($"Could not genereate unique path for {path}");
                    }
                }

                path = string.Format("{0}/{1}_{2}{3}", folder, filename, TurnResolver.Suffix, FileFormat.Asset);
                var turnPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (string.IsNullOrEmpty(turnPath))
                {
                    throw new Exception($"Could not genereate unique path for {path}");
                }

                path = string.Format("{0}/{1}_{2}{3}", folder, filename, ActionRules.Suffix, FileFormat.Asset);
                var actionsPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (string.IsNullOrEmpty(actionsPath))
                {
                    throw new Exception($"Could not genereate unique path for {path}");
                }

                path = string.Format("{0}/{1}_{2}{3}", folder, filename, HealthConvertRules.Suffix, FileFormat.Asset);
                var healthPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (string.IsNullOrEmpty(healthPath))
                {
                    throw new Exception($"Could not genereate unique path for {path}");
                }

                path = string.Format("{0}/{1}_{2}{3}", folder, filename, BattleFinishHandler.Suffix, FileFormat.Asset);
                var battleFinishPath = AssetDatabase.GenerateUniqueAssetPath(path);
                if (string.IsNullOrEmpty(battleFinishPath))
                {
                    throw new Exception($"Could not genereate unique path for {path}");
                }

                newLevel = Object.Instantiate(level);
                AssetDatabase.CreateAsset(newLevel, levelPath);

                if (level.Map)
                {
                    MapSettings newMap = Object.Instantiate(level.Map);
                    newLevel.Map = newMap;
                    AssetDatabase.CreateAsset(newLevel.Map, mapPath);
                }

                if (level.Turn)
                {
                    TurnResolver newTurn = Object.Instantiate(level.Turn);
                    newLevel.Turn = newTurn;
                    AssetDatabase.CreateAsset(newLevel.Turn, turnPath);
                }

                if (level.Actions)
                {
                    ActionRules newActions = Object.Instantiate(level.Actions);
                    newLevel.Actions = newActions;
                    AssetDatabase.CreateAsset(newLevel.Actions, actionsPath);
                }

                if (level.Health)
                {
                    HealthConvertRules newHealth = Object.Instantiate(level.Health);
                    newLevel.Health = newHealth;
                    AssetDatabase.CreateAsset(newLevel.Health, healthPath);
                }

                if (level.BattleFinish)
                {
                    BattleFinishHandler newBattleFinish = Object.Instantiate(level.BattleFinish);
                    newLevel.BattleFinish = newBattleFinish;
                    AssetDatabase.CreateAsset(newLevel.BattleFinish, battleFinishPath);
                }

                AssetDatabase.SaveAssets();
                Log.I($"Level was to duplicated to {levelPath}");
                return newLevel;
            }
            catch (Exception e)
            {
                Log.E($"Level duplication failed. {e.Message}");
                return newLevel;
            }
            finally
            {
                AssetDatabase.Refresh();

            }
        }

        public static LevelData[] FindAll()
        {
            var guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(LevelData).Name));
            var levels = new LevelData[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                levels[i] = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            }
            return levels;
        }
#endif
    }
}
