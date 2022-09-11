using RedBjorn.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Tag represents Team 
    /// </summary>
    [CreateAssetMenu(menuName = Paths.ScriptablePath.Tags.Team)]
    public class TeamTag : Tag
    {
        public static TeamTag Find(string filter = " 1")
        {
#if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(TeamTag).Name, filter));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var data = AssetDatabase.LoadAssetAtPath<TeamTag>(path);
                return data;
            }

            guids = AssetDatabase.FindAssets(string.Concat("t:", typeof(TeamTag).Name));
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var data = AssetDatabase.LoadAssetAtPath<TeamTag>(path);
                return data;
            }
#endif
            return null;
        }
    }
}
