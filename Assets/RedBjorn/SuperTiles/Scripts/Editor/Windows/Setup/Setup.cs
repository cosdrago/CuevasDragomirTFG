using UnityEditor;

namespace RedBjorn.SuperTiles.Editors.Setup
{
    [InitializeOnLoad]
    public class Setup
    {
        static Setup()
        {
            if (EditorPrefs.HasKey(SetupWindowSettings.SetupKeyOld))
            {
                var existed = EditorPrefs.GetBool(SetupWindowSettings.SetupKeyOld);
                EditorPrefs.SetBool(SetupWindowSettings.SetupKey, existed);
                EditorPrefs.DeleteKey(SetupWindowSettings.SetupKeyOld);
            }

            var versionCurrent = SetupWindowSettings.GetVersion();
            var versionSaved = EditorPrefs.GetString(SetupWindowSettings.MultiplayerVersionKey, null);
            var setupDone = EditorPrefs.GetBool(SetupWindowSettings.SetupMultiplayerKey);
            if (!setupDone || string.IsNullOrEmpty(versionSaved))
            {
                SetupWindow.DoShow(versionSaved, versionCurrent);
            }
            EditorPrefs.SetString(SetupWindowSettings.MultiplayerVersionKey, versionCurrent);
        }
    }
}
