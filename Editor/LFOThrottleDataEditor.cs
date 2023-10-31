using LuxsFlamesAndOrnaments.Monobehaviours;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LFO.Editor
{
    [CustomEditor(typeof(LFOThrottleData))]
    public class LFOThrottleDataEditor : UnityEditor.Editor
    {
        bool groupDropdown;

        public override void OnInspectorGUI()
        {
            LFOThrottleData lfoThrottleData = (LFOThrottleData)target;

            if (lfoThrottleData.GetComponent<Renderer>().sharedMaterial == null)
            {
                Material mat = new Material(Shader.Find(false ? "LFO/Additive" : "LFOAdditive 2.0"));

                lfoThrottleData.GetComponent<Renderer>().sharedMaterial = mat;
            }
            else if (GUILayout.Button("New Material Instance"))
            {
                Material mat = new Material(lfoThrottleData.GetComponent<Renderer>().sharedMaterial)
                {
                    name = lfoThrottleData.name + " Plume Material"
                };

                lfoThrottleData.GetComponent<Renderer>().sharedMaterial = mat;
                lfoThrottleData.config.ShaderSettings.ShaderName = mat.shader.name;
                lfoThrottleData.config.ShaderSettings.ShaderParams = new Dictionary<string, object>();
            }

            var throttleGroup = lfoThrottleData.gameObject.transform.GetComponentInParent<LFOThrottleDataMasterGroup>();
            if (throttleGroup != null)
            {
                groupDropdown = EditorGUILayout.Foldout(groupDropdown, "Group Controls");
                if (groupDropdown)
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
            }


            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck() && throttleGroup != null)
            {
                UpdateVisuals(throttleGroup);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Seed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("renderer"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("_material"));
            EditorGUI.EndDisabledGroup();

            if (lfoThrottleData.config != null)
            {
                var serPro = serializedObject.FindProperty("config");
                if (serPro != null)
                    EditorGUILayout.PropertyField(serPro);
            }

            serializedObject.ApplyModifiedProperties();
        }


        void UpdateVisuals(LFOThrottleDataMasterGroup throttleGroup)
        {
            throttleGroup.TriggerUpdateVisuals(throttleGroup.GroupThrottle / 100f, throttleGroup.GroupAtmo, 0,
                Vector3.zero);
        }
    }

    public static class Helpers
    {
        [ContextMenu("LFO/New Layer")]
        public static void AddNewLayer()
        {
        }
    }
}

