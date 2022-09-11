using RedBjorn.Utils;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RedBjorn.SuperTiles.Editors.Unit
{
    public interface ISubmenu
    {
        void DrawCommon(UnitWindow window);
        void DrawWork(UnitWindow window);
        bool HasMenu();
    }

    public class EditSubmenu : ISubmenu
    {
        public void DrawCommon(UnitWindow window)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = window.Unit;
            if (GUILayout.Button("Duplicate", GUILayout.Height(25f)))
            {
                window.Submenu = new DuplicateSubmenu(window);
            }
            GUI.enabled = true;
            if (GUILayout.Button("Create", GUILayout.Height(25f)))
            {
                window.Submenu = new CreateSubmenu();
            }
            GUILayout.EndHorizontal();
        }

        public void DrawWork(UnitWindow window)
        {
            var Tab = window.Tab;
            var Unit = window.Unit;
            if (Unit)
            {
                var SerializedObject = window.SerializedObject;
                if (Tab == 0)
                {
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(nameof(UnitData.Avatar)));
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(nameof(UnitData.Model)));
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(nameof(UnitData.UiHolder)));
                }
                else if (Tab == 1)
                {
                    window.StatsScroll = EditorGUILayout.BeginScrollView(window.StatsScroll);
                    bool doBreak = false;
                    var statsProperty = SerializedObject.FindProperty(nameof(UnitData.Stats));
                    var emptyLabel = new GUIContent("");
                    for (int i = 0; i < statsProperty.arraySize; i++)
                    {
                        var prop = statsProperty.GetArrayElementAtIndex(i);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative(nameof(UnitData.StatData.Stat)), emptyLabel);
                        EditorGUILayout.PropertyField(prop.FindPropertyRelative(nameof(UnitData.StatData.Value)), emptyLabel, GUILayout.Width(50f));

                        if (GUILayout.Button("-", GUILayout.Width(30f)))
                        {
                            doBreak = true;
                            statsProperty.DeleteArrayElementAtIndex(i);
                        }
                        GUILayout.EndHorizontal();

                        if (doBreak)
                        {
                            break;
                        }
                    }

                    if (GUILayout.Button("+", GUILayout.Height(20f)))
                    {
                        statsProperty.arraySize++;
                    }

                    EditorGUILayout.EndScrollView();
                }
                else if (Tab == 2)
                {
                    window.ItemsScroll = EditorGUILayout.BeginScrollView(window.ItemsScroll);
                    bool doBreak = false;
                    var statsProperty = SerializedObject.FindProperty(nameof(UnitData.DefaultItems));
                    EditorGUIUtility.labelWidth = 20f;
                    for (int i = 0; i < statsProperty.arraySize; i++)
                    {
                        var prop = statsProperty.GetArrayElementAtIndex(i);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(prop, new GUIContent(i.ToString()));

                        if (GUILayout.Button("Edit", GUILayout.Width(35f)))
                        {
                            var folder = window.CachedRootFolderPath;
                            var item = prop.objectReferenceValue as ItemData;
                            if (string.IsNullOrEmpty(folder))
                            {
                                if (item)
                                {
                                    folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(item));
                                }
                                else
                                {
                                    folder = string.Empty;
                                }
                            }
                            else
                            {
                                folder = Path.Combine(window.CachedRootFolderPath, @"..", "Items");
                            }
                            ItemWindow.DoShow(prop.objectReferenceValue as ItemData, folder);

                        }
                        if (GUILayout.Button("-", GUILayout.Width(30f)))
                        {
                            doBreak = true;
                            statsProperty.DeleteArrayElementAtIndex(i);
                        }
                        GUILayout.EndHorizontal();

                        if (doBreak)
                        {
                            break;
                        }
                    }

                    if (GUILayout.Button("+", GUILayout.Height(20f)))
                    {
                        statsProperty.arraySize++;
                    }

                    EditorGUILayout.EndScrollView();
                }
            }
        }

        public bool HasMenu()
        {
            return true;
        }
    }

    public class CreateSubmenu : ISubmenu
    {
        public CreateSubmenu()
        {

        }

        public void DrawCommon(UnitWindow window)
        {

        }

        public void DrawWork(UnitWindow window)
        {
            if (string.IsNullOrEmpty(window.CachedRootFolderPath))
            {
                window.CachedRootFolderPath = window.Settings.DefaultItemFolder;
            }
            if (string.IsNullOrEmpty(window.CachedUnitName))
            {
                window.CachedUnitName = window.Settings.DefaultItemName;
            }

            EditorGUIUtility.labelWidth = 70f;
            window.CachedRootFolderPath = EditorGUILayout.TextField("Folder Path", window.CachedRootFolderPath);
            window.CachedUnitName = EditorGUILayout.TextField("Filename", window.CachedUnitName);
            EditorGUIUtility.labelWidth = 90f;

            if (GUILayout.Button("Create"))
            {
                var unit = UnitData.Create(window.CachedRootFolderPath, window.CachedUnitName, S.Battle.Tags.Unit.GetDefault());
                if (unit)
                {
                    window.Unit = unit;
                }
                window.Submenu = new EditSubmenu();
            }
            if (GUILayout.Button("Cancel"))
            {
                window.Submenu = new EditSubmenu();
            }
        }

        public bool HasMenu()
        {
            return false;
        }
    }

    public class DuplicateSubmenu : ISubmenu
    {
        public DuplicateSubmenu(UnitWindow window)
        {
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(window.Unit));
            window.CachedRootFolderPath = Path.GetDirectoryName(assetPath);
            window.CachedUnitName = Path.GetFileNameWithoutExtension(assetPath);
        }

        public void DrawCommon(UnitWindow window)
        {

        }

        public void DrawWork(UnitWindow window)
        {
            EditorGUIUtility.labelWidth = 70f;
            window.CachedRootFolderPath = EditorGUILayout.TextField("Folder Path", window.CachedRootFolderPath);
            window.CachedUnitName = EditorGUILayout.TextField("Filename", window.CachedUnitName);
            if (GUILayout.Button("Duplicate"))
            {
                try
                {
                    if (!Directory.Exists(window.CachedRootFolderPath))
                    {
                        Directory.CreateDirectory(window.CachedRootFolderPath);
                    }

                    var path = string.Format("{0}/{1}{2}", window.CachedRootFolderPath, window.CachedUnitName, FileFormat.Asset);
                    var unitPath = AssetDatabase.GenerateUniqueAssetPath(path);
                    if (string.IsNullOrEmpty(unitPath))
                    {
                        throw new Exception($"Could not genereate unique path for {path}");
                    }

                    var unit = Object.Instantiate(window.Unit);
                    AssetDatabase.CreateAsset(unit, unitPath);

                    if (window.Unit.Avatar)
                    {
                        var avatarPath = AssetDatabase.GetAssetPath(window.Unit.Avatar);
                        var avatarPathNew = AssetDatabase.GenerateUniqueAssetPath(avatarPath);
                        if (string.IsNullOrEmpty(avatarPathNew))
                        {
                            throw new Exception($"Could not genereate unique path for {avatarPath}");
                        }
                        AssetDatabase.CopyAsset(avatarPath, avatarPathNew);
                        unit.Avatar = AssetDatabase.LoadAssetAtPath<Sprite>(avatarPathNew);
                    }

                    AssetDatabase.SaveAssets();
                    Log.I($"Unit was to duplicated to {unitPath}");
                    window.Unit = unit;
                }
                catch (Exception e)
                {
                    Log.E($"Unit duplication failed. {e.Message}");
                }
                finally
                {
                    AssetDatabase.Refresh();
                    window.Submenu = new EditSubmenu();
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                window.Submenu = new EditSubmenu();
            }
        }

        public bool HasMenu()
        {
            return false;
        }
    }
}
