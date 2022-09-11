using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.Editors
{
#if !UNITY_2020_3_OR_NEWER
    [CustomEditor(typeof(NetworkSettings))]
    public class NetworkSettingsEditor : Editor
    {
        ReorderableList Reordable;
        SerializedProperty Property;

        void OnEnable()
        {
            Property = serializedObject.FindProperty(nameof(NetworkSettings.Sdks));
            Reordable = new ReorderableList(serializedObject, Property, true, true, true, true);
            Reordable.drawElementCallback = DrawElement;
            Reordable.drawHeaderCallback = DrawHeader;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(true))
            {
                if (Property != null && prop.propertyPath == Property.propertyPath && Reordable != null)
                {
                    Reordable.DoLayoutList();
                }
                else if (prop.depth == 0)
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = Reordable.serializedProperty.GetArrayElementAtIndex(index);
            var position = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(position, element, GUIContent.none);
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, nameof(NetworkSettings.Sdks));
        }
    }
#endif
}
