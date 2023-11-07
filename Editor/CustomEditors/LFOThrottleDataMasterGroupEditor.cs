﻿using KSP;
using LFO.Shared.Components;
using LFO.Shared.Configs;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LFO.Editor.Utility;
using LFO.Shared;
using UnityEditor;
using UnityEngine;
using ILogger = LFO.Shared.ILogger;

namespace LFO.Editor.CustomEditors
{
    [CustomEditor(typeof(LFOThrottleDataMasterGroup))]
    public class LFOThrottleDataMasterGroupEditor : UnityEditor.Editor
    {
        private const string PlumesFolder = "Assets/plugin_template/assets/plumes/";

        private static ILogger Logger => ServiceProvider.GetService<ILogger>();
        private static IAssetManager AssetManager => ServiceProvider.GetService<IAssetManager>();

        private bool _useNewShader;

        public override void OnInspectorGUI()
        {
            var group = (LFOThrottleDataMasterGroup)target;

            _useNewShader = EditorGUILayout.Toggle("Use New Shader", _useNewShader);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Group Throttle");
            group.GroupThrottle = EditorGUILayout.Slider(
                group.GroupThrottle,
                0,
                LFOThrottleDataMasterGroup.ThrottleMax
            );
            EditorGUILayout.LabelField("Group Atmospheric Pressure");
            group.GroupAtmo = EditorGUILayout.Slider(
                group.GroupAtmo,
                0,
                LFOThrottleDataMasterGroup.AtmoMax
            );

            EditorGUI.BeginDisabledGroup(group.GroupAtmo > 0.0092f);
            EditorGUILayout.LabelField("UpperAtmo Fine tune");
            float atmo = EditorGUILayout.Slider(group.GroupAtmo, 0, 0.0092f);
            if (group.GroupAtmo <= 0.0092f)
            {
                group.GroupAtmo = atmo;
            }

            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                var allMasters = FindObjectsOfType<LFOThrottleDataMasterGroup>();

                foreach (var master in allMasters)
                {
                    UpdateVisuals(master);
                }
            }

            EditorGUI.BeginChangeCheck();
            group.Active = EditorGUILayout.Toggle("Active?", group.Active);
            if (EditorGUI.EndChangeCheck())
            {
                group.ToggleVisibility(group.Active);
                EditorUtility.SetDirty(group);
            }

            GUILayout.Label($"{group.ThrottleDatas.Count} children");
            if (GUILayout.Button("Collect children"))
            {
                group.ThrottleDatas = group.GetComponentsInChildren<LFOThrottleData>(true).ToList();
            }

            var partData = group.GetComponentInParent<CorePartData>();
            var filename = partData != null ? $"{partData.Data.partName}.json" : $"{group.name}.json";

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Save to JSON"))
            {
                HandleSaveConfig(group, partData, filename);
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Reload materials from JSON"))
            {
                HandleReloadConfig(group, filename);
            }

            if (GUILayout.Button("Load from JSON"))
            {
                if (EditorUtility.DisplayDialog(
                        "Warning",
                        "This will remove all child objects of this group and recreate them from JSON. Continue?",
                        "Load",
                        "Cancel"
                    ))
                {
                    HandleLoadConfig(group, filename);
                }
            }
        }

        private void HandleLoadConfig(LFOThrottleDataMasterGroup group, string filename)
        {
            LFOConfig lfoConfig = LoadFromJson(PlumesFolder, filename);
            PlumeUtility.CreatePlumeFromConfig(lfoConfig, group.gameObject.transform.parent.gameObject);
            group.gameObject.DestroyGameObjectImmediate();
        }

        private static void HandleSaveConfig(
            LFOThrottleDataMasterGroup group,
            CorePartData partData,
            string filename
        )
        {
            var config = new LFOConfig();
            if (partData != null)
            {
                config.PartName = group.GetComponentInParent<CorePartData>().Data.partName;
            }

            config.PlumeConfigs = new Dictionary<string, List<PlumeConfig>>();

            foreach (var throttleData in group.GetComponentsInChildren<LFOThrottleData>())
            {
                var plumeConfig = new PlumeConfig();
                Material material = throttleData.GetComponent<Renderer>().sharedMaterial;
                Transform transform = throttleData.transform;

                plumeConfig.ShaderSettings = ShaderConfig.GenerateConfig(material);
                plumeConfig.Position = transform.localPosition;
                plumeConfig.Scale = transform.localScale;
                plumeConfig.Rotation = transform.localRotation.eulerAngles;
                plumeConfig.FloatParams = throttleData.FloatParams;
                plumeConfig.MeshPath = throttleData.TryGetComponent(out SkinnedMeshRenderer skinnedRenderer)
                    ? skinnedRenderer.sharedMesh.name
                    : throttleData.GetComponent<MeshFilter>().sharedMesh.name;
                plumeConfig.TargetGameObject = throttleData.name;

                if (!config.PlumeConfigs.ContainsKey(throttleData.transform.parent.name))
                {
                    config.PlumeConfigs.Add(throttleData.transform.parent.name, new List<PlumeConfig>());
                }

                config.PlumeConfigs[throttleData.transform.parent.name].Add(plumeConfig);

                throttleData.Config = plumeConfig;
            }

            group.StartCoroutine(SaveToJson(config, PlumesFolder, filename));
        }

        private void HandleReloadConfig(LFOThrottleDataMasterGroup group, string filename)
        {
            LFOConfig lfoConfig = LoadFromJson(PlumesFolder, filename);
            foreach (var throttleData in group.GetComponentsInChildren<LFOThrottleData>())
            {
                int index = lfoConfig.PlumeConfigs[throttleData.transform.parent.name]
                    .FindIndex(a => a.TargetGameObject == throttleData.name);
                if (index < 0)
                {
                    continue;
                }

                throttleData.Config = lfoConfig.PlumeConfigs[throttleData.transform.parent.name][index];

                throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.Config.GetEditorMaterial();
                if (_useNewShader)
                {
                    throttleData.GetComponent<Renderer>().sharedMaterial.shader =
                        AssetManager.GetShader("LFO/Additive");
                }

                throttleData.GetComponent<Renderer>().sharedMaterial.name =
                    throttleData.name + " Plume Material";
            }
        }

        private static void UpdateVisuals(LFOThrottleDataMasterGroup throttleGroup)
        {
            if (!throttleGroup.Active)
            {
                return;
            }

            throttleGroup.ThrottleDatas.ForEach(throttleData =>
            {
                if (!throttleData.IsVisible())
                {
                    return;
                }

                if (throttleData.Config == null)
                {
                    Logger.LogWarning($"Config for {throttleData.name} is null");
                    return;
                }

                throttleData.TriggerUpdateVisuals(
                    throttleGroup.GroupThrottle / 100f,
                    throttleGroup.GroupAtmo,
                    0,
                    Vector3.zero
                );
            });
        }

        private static LFOConfig LoadFromJson(string path, string filename)
        {
            if (File.Exists(Path.Combine(path, filename)))
            {
                string rawJsonExisting = File.OpenText(Path.Combine(path, filename)).ReadToEnd();
                return LFOConfig.Deserialize(rawJsonExisting);
            }

            if (!Directory.Exists(path))
            {
                path = "Assets";
            }

            string rawJson = File.OpenText(EditorUtility.OpenFilePanel(
                "LFO Config File",
                path,
                "json"
            )).ReadToEnd();

            return LFOConfig.Deserialize(rawJson);
        }

        private static IEnumerator SaveToJson(LFOConfig config, string path, string fileName)
        {
            Directory.CreateDirectory(path);

            string json = LFOConfig.Serialize(config);

            using (StreamWriter sw = File.CreateText(Path.Combine(path, fileName)))
            {
                sw.Write(json);
            }

            yield return null;

            AssetDatabase.Refresh();
        }
    }
}