﻿using RedBjorn.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors.Level.Submenus.Tabs
{
    [Serializable]
    public class CameraTab : ITab
    {
        public CameraTab(LevelWindow window)
        {

        }

        public void Draw(LevelWindow window)
        {
            GUI.enabled = true;
            var common = window.Submenu.GetType() == typeof(Edit);
            if (window.Level && common)
            {
                EditorGUIUtility.labelWidth = 120f;
                EditorWindowExtended.DrawProperties(window.SerializedObject.FindProperty(nameof(LevelData.Camera)), true);
            }
        }
    }
}
