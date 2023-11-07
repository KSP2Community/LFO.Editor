using LFO.Shared.Components;
using System.Collections.Generic;
using LFO.Shared;
using UnityEditor;
using UnityEngine;

namespace LFO.Editor.CustomEditors
{
    [CustomEditor(typeof(LFOThrottleData))]
    public class LFOThrottleDataEditor : UnityEditor.Editor
    {
        private static IAssetManager AssetManager => ServiceProvider.GetService<IAssetManager>();
        private bool _groupDropdown;

        public override void OnInspectorGUI()
        {
            var lfoThrottleData = (LFOThrottleData)target;
            var renderer = lfoThrottleData.GetComponent<Renderer>();

            if (renderer.sharedMaterial == null)
            {
                var mat = new Material(AssetManager.GetShader("LFO/Additive"));
                renderer.sharedMaterial = mat;
            }
            else if (GUILayout.Button("New Material Instance"))
            {
                var mat = new Material(renderer.sharedMaterial)
                {
                    name = lfoThrottleData.name + " Plume Material"
                };

                renderer.sharedMaterial = mat;
                lfoThrottleData.Config.ShaderSettings.ShaderName = mat.shader.name;
                lfoThrottleData.Config.ShaderSettings.ShaderParams = new Dictionary<string, object>();
            }

            var throttleGroup = lfoThrottleData.gameObject.transform.GetComponentInParent<LFOThrottleDataMasterGroup>();
            if (throttleGroup != null)
            {
                _groupDropdown = EditorGUILayout.Foldout(_groupDropdown, "Group Controls");
                if (_groupDropdown)
                {
                    HandleGroupControls(throttleGroup);
                }
            }

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck() && throttleGroup != null)
            {
                UpdateVisuals(throttleGroup);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Seed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Renderer"));
            EditorGUI.EndDisabledGroup();

            if (lfoThrottleData.Config != null)
            {
                var serializedProperty = serializedObject.FindProperty("Config");
                if (serializedProperty != null)
                {
                    EditorGUILayout.PropertyField(serializedProperty);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void HandleGroupControls(LFOThrottleDataMasterGroup throttleGroup)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Group Throttle");
            throttleGroup.GroupThrottle = EditorGUILayout.Slider(throttleGroup.GroupThrottle, 0, 100f);
            EditorGUILayout.LabelField("Group Atmospheric Pressure");
            throttleGroup.GroupAtmo = EditorGUILayout.Slider(throttleGroup.GroupAtmo, 0, 1.1f);

            EditorGUI.BeginDisabledGroup(throttleGroup.GroupAtmo > 0.0092f);
            EditorGUILayout.LabelField("UpperAtmo Fine tune");
            float atmo = EditorGUILayout.Slider(throttleGroup.GroupAtmo, 0, 0.0092f);
            if (throttleGroup.GroupAtmo <= 0.0092f)
            {
                throttleGroup.GroupAtmo = atmo;
            }

            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateVisuals(throttleGroup);
            }
        }

        private static void UpdateVisuals(LFOThrottleDataMasterGroup throttleGroup)
        {
            throttleGroup.TriggerUpdateVisuals(
                throttleGroup.GroupThrottle / 100f,
                throttleGroup.GroupAtmo,
                0,
                Vector3.zero
            );
        }
    }
}