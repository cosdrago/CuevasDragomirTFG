using RedBjorn.Utils;
using UnityEditor;

namespace RedBjorn.SuperTiles.Editors.Item.Tabs
{
    public class Visual : ITab
    {
        public Visual(ItemWindow window)
        {

        }

        public void Draw(ItemWindow window)
        {
            EditorGUIUtility.labelWidth = 135f;
            if (window.SerializedObject != null)
            {
                EditorGUILayout.PropertyField(window.SerializedObject.FindProperty(nameof(ItemData.Caption)));
                EditorWindowExtended.DrawProperties(window.SerializedObject.FindProperty(nameof(ItemData.Visual)), true);
            }
        }
    }
}
