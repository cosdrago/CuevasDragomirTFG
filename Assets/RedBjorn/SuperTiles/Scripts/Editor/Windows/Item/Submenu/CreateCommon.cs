﻿using RedBjorn.SuperTiles.Items.ActionHandlers;
using RedBjorn.SuperTiles.Items.TargetSelectors;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors.Item
{
    public class CreateCommon : ISubmenu
    {
        Rect Common;
        Rect CommonContent;
        Rect WorkArea;
        Rect WorkAreaContent;

        public void Draw(ItemWindow window)
        {
            var Border = window.Border;
            //Common
            Common.x = 2 * Border;
            Common.y = 2 * Border;
            Common.width = window.WindowWidth - 4 * Border;
            Common.height = window.CommonHeight;
            EditorGUI.DrawRect(Common, window.CommonColor);
            CommonContent.x = Common.x + 2 * Border;
            CommonContent.y = Common.y + 2 * Border;
            CommonContent.width = Common.width - 4 * Border;
            CommonContent.height = Common.height - 4 * Border;

            GUILayout.BeginArea(CommonContent);
            GUILayout.BeginHorizontal();
            window.Item = EditorGUILayout.ObjectField("Item Asset", window.Item, typeof(ItemData), allowSceneObjects: false) as ItemData;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var enabled = GUI.enabled;
            GUI.enabled = window.Item != null;
            if (GUILayout.Button("Duplicate", GUILayout.Height(25f)))
            {
                window.Submenu = new Duplicate();
                var path = AssetDatabase.GetAssetPath(window.Item);
                window.CachedRootFolderPath = Path.GetDirectoryName(Path.GetDirectoryName(path));
                window.CachedItemName = Path.GetFileNameWithoutExtension(path) + "New";
            }
            GUI.enabled = enabled;
            if (GUILayout.Button("Create", GUILayout.Height(25f)))
            {
                window.Submenu = new CreateCommon();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //Work
            WorkArea.x = Common.x;
            WorkArea.y = Common.y + Common.height + 2 * Border;
            WorkArea.width = window.WindowWidth - 4 * Border;
            WorkArea.height = window.WindowHeight - Common.y - Common.height - 10 * Border;
            EditorGUI.DrawRect(WorkArea, window.WorkAreaColor);
            WorkAreaContent.x = WorkArea.x + 2 * Border;
            WorkAreaContent.y = WorkArea.y + 2 * Border;
            WorkAreaContent.width = WorkArea.width - 4 * Border;
            WorkAreaContent.height = WorkArea.height - 4 * Border;

            GUILayout.BeginArea(WorkAreaContent);
            window.CachedRootFolderPath = EditorGUILayout.TextField("Path", window.CachedRootFolderPath);
            window.CachedItemName = EditorGUILayout.TextField("Item Name", window.CachedItemName);

            GUILayout.BeginHorizontal();
            var height = GUILayout.Height(50f);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(height);
            if (GUILayout.Button("Melee", height))
            {
                window.ItemCreateNew(typeof(RangeTargetSelector).Name, typeof(Melee).Name);
                window.Submenu = new Edit(window);
            }
            if (GUILayout.Button("Range", height))
            {
                window.ItemCreateNew(typeof(RangeTargetSelector).Name, typeof(Bullet).Name);
                window.Submenu = new Edit(window);
            }
            if (GUILayout.Button("Direction", height))
            {
                window.ItemCreateNew(typeof(DirectionTargetSelector).Name, typeof(Laser).Name);
                window.Submenu = new Edit(window);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Custom", GUILayout.Height(25f)))
            {
                window.UpdatePopUp();
                window.Submenu = new CreateCustom();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Cancel", GUILayout.Height(25f)))
            {
                window.Submenu = new Edit(window);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}