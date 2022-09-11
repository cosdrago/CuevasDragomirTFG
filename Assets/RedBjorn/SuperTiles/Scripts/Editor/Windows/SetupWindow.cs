using RedBjorn.SuperTiles.Editors.Setup;
using RedBjorn.Utils;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RedBjorn.SuperTiles.Editors
{
    [Serializable]
    public class SetupWindow : EditorWindowExtended
    {
        [MenuItem("Tools/Red Bjorn/SuperTiles/Setup")]
        public static void DoShow()
        {
            var version = SetupWindowSettings.GetVersion();
            DoShow(version, version);
        }

        public static void DoShow(string versionSaved, string versionCurrent)
        {
            var window = (SetupWindow)GetWindow(typeof(SetupWindow));
            window.titleContent = new GUIContent("Setup");
            window.minSize = new Vector2(320f, 360f);
            window.Show();
        }

        public Vector2 ScrollPos;

        string WelcomeText = @"Thank you for choosing SuperTiles asset

There are several essentials that should be done to startup up asset properly:";

        string TextMeshProText = @"1. Import Text Mesh Pro Essentials.
Default Text Mesh Pro fonts and components are used.";

        string SetupText = @"2. Add asset scenes to Build Settings -> Scenes in Build. 
(!) Existed scenes will stay on this list, only new scenes will be added.";

        string PhotonText = @"3. Photon SDK. This sdk is necessary for 
the correct functioning of the multiplayer.";

        string ItemText = @"4. (optional) Update items created in version lower 1.2.0.
Item.Visual.SelectorMaterial will be copied to SelectorGenerated
Item.Visual.Available (for available tiles) will be filled with default values";

        string BattleFinishText = @"5. (optional) Create Battle finish condition assets if there are no";

        void OnGUI()
        {
            var contentColor = GUI.color;
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90;
            var gui = GUI.enabled;
            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
            EditorGUILayout.LabelField(WelcomeText, EditorStyles.textArea, GUILayout.Height(70f));
            GUILayout.Space(20f);

            EditorGUILayout.LabelField(TextMeshProText, EditorStyles.textArea, GUILayout.Height(40f));
            ShowStatus(File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"));
            GUI.color = contentColor;
            if (GUILayout.Button("Import", GUILayout.Height(30f)))
            {
                TMP_PackageResourceImporterWindow.ShowPackageImporterWindow();
            }
            GUILayout.Space(20f);

            GUI.enabled = true;
            EditorGUILayout.LabelField(SetupText, EditorStyles.textArea, GUILayout.Height(70f));
            ShowStatus(EditorPrefs.GetBool(SetupWindowSettings.SetupMultiplayerKey));
            GUI.color = contentColor;
            if (GUILayout.Button("Add", GUILayout.Height(30f)))
            {
                SetupWindowSettings.AddScenesToBuildSettings();
            }
            GUILayout.Space(20f);

            GUI.enabled = true;
            EditorGUILayout.LabelField(PhotonText, EditorStyles.textArea, GUILayout.Height(70f));
            ShowStatus(HavePhotonSdk());
            GUI.color = contentColor;
            if (GUILayout.Button("Open Asset Store link", GUILayout.Height(30f)))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/network/pun-2-free-119922");
            }
            GUILayout.Space(20f);

            GUI.enabled = true;
            EditorGUILayout.LabelField(ItemText, EditorStyles.textArea, GUILayout.Height(80f));
            if (GUILayout.Button("Update Items", GUILayout.Height(30f)))
            {
                SetupWindowSettings.ItemsUpdate();
            }

            GUILayout.Space(20f);

            GUI.enabled = true;
            EditorGUILayout.LabelField(BattleFinishText, EditorStyles.textArea, GUILayout.Height(80f));
            ShowStatus(LevelData.FindAll().All(l => l.BattleFinish));
            GUI.color = contentColor;
            if (GUILayout.Button("Create Battle Finish", GUILayout.Height(30f)))
            {
                SetupWindowSettings.BattleFinishCreate();
            }

            EditorGUILayout.EndScrollView();
            EditorGUIUtility.labelWidth = labelWidth;
            GUI.enabled = gui;
            GUI.color = contentColor;
        }

        void ShowStatus(bool status)
        {
            if (status)
            {
                GUI.color = Color.green;
                GUILayout.Label("Done", EditorStyles.helpBox);
                GUI.enabled = false;
            }
            else
            {
                GUI.color = Color.red;
                GUILayout.Label("Not done", EditorStyles.helpBox);
                GUI.enabled = true;
            }
        }

        bool HavePhotonSdk()
        {
#if PHOTON_UNITY_NETWORKING
            return true;
#else
            return false;
#endif
        }
    }
}
