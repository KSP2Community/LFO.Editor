using KSP;
using LFO.Shared.Components;
using LFO.Shared.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LFO.Editor
{
    [CustomEditor(typeof(LfoThrottleDataMasterGroup))]
    public class LFOThrottleDataMasterGroupEditor : UnityEditor.Editor
    {
        public bool UseNewShader;

        public override void OnInspectorGUI()
        {
            var group = (LfoThrottleDataMasterGroup)target;

            UseNewShader = EditorGUILayout.Toggle("Use New Shader", UseNewShader);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Group Throttle");
            group.GroupThrottle = EditorGUILayout.Slider(group.GroupThrottle, 0, 100f);
            EditorGUILayout.LabelField("Group Atmospheric Pressure");
            group.GroupAtmo = EditorGUILayout.Slider(group.GroupAtmo, 0, 1.1f);

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
                var allMasters = FindObjectsOfType<LfoThrottleDataMasterGroup>();

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
                group.ThrottleDatas = group.GetComponentsInChildren<LfoThrottleData>(true).ToList();
            }

            var partData = group.GetComponentInParent<CorePartData>();
            var path = $"{Application.dataPath}/plugin_template/assets/plumes/";
            var fileName = partData != null ? $"{partData.Data.partName}.json" : $"{group.name}.json";

            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Save config"))
                {
                    var config = new LFOConfig();
                    if (partData != null)
                    {
                        config.PartName = group.GetComponentInParent<CorePartData>().Data.partName;
                    }

                    config.PlumeConfigs = new Dictionary<string, List<PlumeConfig>>();

                    foreach (var throttleData in group.GetComponentsInChildren<LfoThrottleData>())
                    {
                        var plumeConfig = new PlumeConfig();
                        Material material = throttleData.GetComponent<Renderer>().sharedMaterial;
                        Shader shader = material.shader;
                        Transform transform = throttleData.transform;

                        plumeConfig.ShaderSettings = ShaderConfig.GenerateConfig(material);
                        plumeConfig.Position = transform.localPosition;
                        plumeConfig.Scale = transform.localScale;
                        plumeConfig.Rotation = transform.localRotation.eulerAngles;
                        plumeConfig.FloatParams = throttleData.FloatParams;
                        plumeConfig.MeshPath = throttleData.GetComponent<MeshFilter>().sharedMesh.name;
                        plumeConfig.TargetGameObject = throttleData.name;

                        if (!config.PlumeConfigs.ContainsKey(throttleData.transform.parent.name))
                        {
                            config.PlumeConfigs.Add(throttleData.transform.parent.name, new List<PlumeConfig>());
                        }

                        config.PlumeConfigs[throttleData.transform.parent.name].Add(plumeConfig);

                        throttleData.Config = plumeConfig;
                    }

                    group.StartCoroutine(SaveToJson(config, path, fileName));
                }

                if (GUILayout.Button("Reload config"))
                {
                    foreach (var throttleData in group.GetComponentsInChildren<LfoThrottleData>())
                    {
                        throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.Config.GetMaterial();
                    }
                }
            }
            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Load config"))
                {
                    var plumeconfig = LoadFromJson(path, fileName);
                    foreach (var throttleData in group.GetComponentsInChildren<LfoThrottleData>())
                    {
                        int index = plumeconfig.PlumeConfigs[throttleData.transform.parent.name]
                            .FindIndex(a => a.TargetGameObject == throttleData.name);
                        if (index < 0)
                        {
                            continue;
                        }

                        throttleData.Config = plumeconfig.PlumeConfigs[throttleData.transform.parent.name][index];

                        throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.Config.GetEditorMaterial();
                        if (UseNewShader)
                        {
                            throttleData.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("LFO/Additive");
                        }

                        throttleData.GetComponent<Renderer>().sharedMaterial.name =
                            throttleData.name + " Plume Material";
                    }
                }
            }
        }

        void UpdateVisuals(LfoThrottleDataMasterGroup throttleGroup)
        {
            if (throttleGroup.Active)
            {
                throttleGroup.ThrottleDatas.ForEach(a =>
                {
                    if (!a.IsVisible())
                    {
                        return;
                    }

                    if (a.Config == null)
                    {
                        Debug.LogWarning($"Config for {a.name} is null");
                    }
                    else
                    {
                        a.TriggerUpdateVisuals(
                            throttleGroup.GroupThrottle / 100f,
                            throttleGroup.GroupAtmo,
                            0,
                            Vector3.zero
                        );
                    }
                });
            }
        }

        private static LFOConfig LoadFromJson(string path, string fileName)
        {
            string rawJson = File.OpenText(Path.Combine(path, fileName)).ReadToEnd();

            return LFOConfig.Deserialize(rawJson);
        }

        private static IEnumerator SaveToJson(LFOConfig config, string path, string fileName)
        {
            Directory.CreateDirectory(path);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = LFOConfig.Serialize(config);

            using (StreamWriter sw = File.CreateText(Path.Combine(path, fileName)))
            {
                sw.Write(json);
            }

            var fromJson = JsonConvert.DeserializeObject<LFOConfig>(json);

            yield return null;

            AssetDatabase.Refresh();
        }
    }

    public static class Extensions
    {
        public static Material GetEditorMaterial(this PlumeConfig config)
        {
            var shader = Shader.Find(config.ShaderSettings.ShaderName);
            if (shader is null)
            {
                Debug.LogError($"Couldn't find shader {config.ShaderSettings.ShaderName}");
                return null;
            }

            var material = new Material(shader);

            foreach (var kvp in config.ShaderSettings.ShaderParams)
            {
                if (kvp.Value is JObject jobject)
                {
                    if (jobject.ContainsKey("r"))
                    {
                        var color = new Color(
                            jobject["r"].ToObject<float>(),
                            jobject["g"].ToObject<float>(),
                            jobject["b"].ToObject<float>(),
                            jobject["a"].ToObject<float>()
                        );
                        material.SetColor(kvp.Key, color);
                    }
                    else if (jobject.ContainsKey("x"))
                    {
                        var vector = Vector4.zero;

                        vector.x = jobject["x"].ToObject<float>();
                        vector.y = jobject["y"].ToObject<float>();

                        if (jobject.ContainsKey("z"))
                        {
                            vector.z = jobject["z"].ToObject<float>();
                        }

                        if (jobject.ContainsKey("w"))
                        {
                            vector.w = jobject["w"].ToObject<float>();
                        }

                        material.SetVector(kvp.Key, vector);
                    }
                }
                else
                {
                    switch (kvp.Value)
                    {
                        case Color color:
                            material.SetColor(kvp.Key, color);
                            break;
                        case Vector2 vector2:
                            material.SetVector(kvp.Key, vector2);
                            break;
                        case Vector3 vector3:
                            material.SetVector(kvp.Key, vector3);
                            break;
                        case Vector4 vector4:
                            material.SetVector(kvp.Key, vector4);
                            break;
                        case float number:
                            material.SetFloat(kvp.Key, number);
                            break;
                        case double dnumber:
                            material.SetFloat(kvp.Key, (float)dnumber);
                            break;
                        case int integer:
                            material.SetFloat(kvp.Key, integer);
                            break;
                        case string textureName:
                            string path = Path.Combine(
                                "Packages",
                                "lfo.editor",
                                "Assets",
                                "Noise",
                                textureName + ".png"
                            );
                            Debug.Log(path);
                            material.SetTexture(kvp.Key, AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                            break;
                    }
                }
            }

            return material;
        }
    }
}