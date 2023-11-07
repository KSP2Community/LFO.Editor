using UnityEditor;
using UnityEngine;
using LFO.Editor.Utility;
using LFO.Shared.Components;
using LFO.Shared.ShaderEditor;

namespace LFO.Editor.CustomEditors
{
    [CustomPropertyDrawer(typeof(ParamName))]
    public class FloatParamDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var paramNameProp = property.FindPropertyRelative("Value");
            if (property.serializedObject.targetObject is not LFOThrottleData { Renderer: { } renderer })
            {
                EditorGUI.PropertyField(nameRect, paramNameProp, GUIContent.none);
            }
            else
            {
                string[] shaderProps = ShaderUtility.GetShaderPropertyNames(renderer.sharedMaterial.shader);

                if (shaderProps != null)
                {
                    if (EditorGUI.DropdownButton(
                            nameRect,
                            new GUIContent(paramNameProp.stringValue),
                            FocusType.Keyboard
                        ))
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (string propName in shaderProps)
                        {
                            bool isSelected = paramNameProp.stringValue == propName;
                            menu.AddItem(new GUIContent(propName), isSelected, () =>
                            {
                                paramNameProp.stringValue = propName;
                                paramNameProp.serializedObject.ApplyModifiedProperties();
                            });
                        }

                        menu.ShowAsContext();
                    }
                }
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}