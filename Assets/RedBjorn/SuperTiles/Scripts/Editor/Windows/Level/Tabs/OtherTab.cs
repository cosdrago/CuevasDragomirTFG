using System;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors.Level.Submenus.Tabs
{
    [Serializable]
    public class OtherTab : ITab
    {
        public OtherTab(LevelWindow window)
        {

        }

        public void Draw(LevelWindow window)
        {
            GUI.enabled = true;
            EditorGUIUtility.labelWidth = 200f;
            if (window.SerializedObject != null && window.SerializedObject.targetObject != null)
            {
                EditorGUILayout.PropertyField(window.SerializedObject.FindProperty(nameof(LevelData.AutoStart)), true);
                EditorGUILayout.PropertyField(window.SerializedObject.FindProperty(nameof(LevelData.EqualEffectInfluenceDuration)), true);
            }
        }
    }
}
