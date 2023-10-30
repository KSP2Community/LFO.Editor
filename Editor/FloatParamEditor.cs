using UnityEditor;
using UnityEngine;

namespace LFO.Editor
{
    [CustomPropertyDrawer(typeof(CurveType))]
    internal class CurveTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = (int)(CurveType)EditorGUI.EnumPopup(position, label, (CurveType)property.intValue);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;
            return EditorGUIUtility.singleLineHeight * lineCount +
                   EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);
        }
    }
}