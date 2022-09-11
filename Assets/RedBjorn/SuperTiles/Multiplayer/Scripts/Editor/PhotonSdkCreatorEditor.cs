using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RedBjorn.SuperTiles.Multiplayer.Sdk.Photon.Editors
{
# if !UNITY_2020_3_OR_NEWER
    [CustomEditor(typeof(PhotonSdkCreator))]
    public class PhotonSdkCreatorEditor : Editor
    {
        ReorderableList Reordable;
        SerializedProperty Property;

        void OnEnable()
        {
            Property = serializedObject.FindProperty(nameof(PhotonSdkCreator.Servers));   
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
                if(Property != null && prop.propertyPath == Property.propertyPath && Reordable != null)
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
            var position = new Rect(rect.x, rect.y, 50f, EditorGUIUtility.singleLineHeight);
            var name = nameof(Server.Name);
            EditorGUI.LabelField(position, name);
            position.x += position.width;
            position.width = 100f;
            EditorGUI.PropertyField(position, element.FindPropertyRelative(name), GUIContent.none);
            position.x += position.width + 20;
            position.width = 40f;
            var code = nameof(Server.Code);
            EditorGUI.LabelField(position, code);
            position.x += position.width;
            position.width = 50f;
            EditorGUI.PropertyField(position, element.FindPropertyRelative(code), GUIContent.none);
        }

        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, nameof(PhotonSdkCreator.Servers));
        }
    }
# endif
}
