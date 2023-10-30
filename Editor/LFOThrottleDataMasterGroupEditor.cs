using KSP;
using LuxsFlamesAndOrnaments.Monobehaviours;
using LuxsFlamesAndOrnaments.Settings;
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
    [CustomEditor(typeof(LFOThrottleDataMasterGroup))]
    public class LFOThrottleDataMasterGroupEditor : UnityEditor.Editor
    {
        public bool UseNewShader;

        public override void OnInspectorGUI()
        {
            LFOThrottleDataMasterGroup target = (LFOThrottleDataMasterGroup)base.target;

            UseNewShader = EditorGUILayout.Toggle("Use New Shader", UseNewShader);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Group Throttle");
            target.GroupThrottle = EditorGUILayout.Slider(target.GroupThrottle, 0, 100f);
            EditorGUILayout.LabelField("Group Atmospheric Pressure");
            target.GroupAtmo = EditorGUILayout.Slider(target.GroupAtmo, 0, 1.1f);

            EditorGUI.BeginDisabledGroup(target.GroupAtmo > 0.0092f);
            EditorGUILayout.LabelField("UpperAtmo Fine tune");
            float atmo = EditorGUILayout.Slider(target.GroupAtmo, 0, 0.0092f);
            if (target.GroupAtmo <= 0.0092f)
            {
                target.GroupAtmo = atmo;
            }

            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                var allMasters = GameObject.FindObjectsOfType<LFOThrottleDataMasterGroup>();

                foreach (var master in allMasters)
                {
                    UpdateVisuals(master);
                }
            }

            EditorGUI.BeginChangeCheck();
            target.Active = EditorGUILayout.Toggle("Active?", target.Active);
            if (EditorGUI.EndChangeCheck())
            {
                target.ToggleVisibility(target.Active);
                EditorUtility.SetDirty(target);
            }

            GUILayout.Label($"{target.throttleDatas.Count} children");
            if (GUILayout.Button("Collect children"))
            {
                target.throttleDatas = target.GetComponentsInChildren<LFOThrottleData>(true).ToList();
            }

            var partData = target.GetComponentInParent<CorePartData>();
            var path = $"{Application.dataPath}/LFO/configs/";
            string fileName;
            if (partData != null)
            {
                fileName = $"{partData.Data.partName}.json";
            }
            else
            {
                fileName = $"{target.name}.json";
            }

            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Save config"))
                {
                    LFOConfig config = new LFOConfig();
                    if (partData != null)
                        config.partName = target.GetComponentInParent<CorePartData>().Data.partName;
                    config.PlumeConfigs = new Dictionary<string, List<PlumeConfig>>();

                    foreach (LFOThrottleData throttleData in target.GetComponentsInChildren<LFOThrottleData>())
                    {
                        PlumeConfig plumeConfig = new PlumeConfig();
                        Material material = throttleData.GetComponent<Renderer>().sharedMaterial;
                        Shader shader = material.shader;
                        plumeConfig.ShaderSettings = ShaderConfig.GenerateConfig(material);
                        plumeConfig.Position = throttleData.transform.localPosition;
                        plumeConfig.Scale = throttleData.transform.localScale;
                        plumeConfig.Rotation = throttleData.transform.localRotation.eulerAngles;
                        plumeConfig.FloatParams = throttleData.FloatParams;
                        plumeConfig.meshPath = throttleData.GetComponent<MeshFilter>().sharedMesh.name;
                        plumeConfig.targetGameObject = throttleData.name;
                        if (!config.PlumeConfigs.ContainsKey(throttleData.transform.parent.name))
                            config.PlumeConfigs.Add(throttleData.transform.parent.name, new List<PlumeConfig>());

                        config.PlumeConfigs[throttleData.transform.parent.name].Add(plumeConfig);

                        throttleData.config = plumeConfig;
                    }

                    target.StartCoroutine(SaveToJson(config, path, fileName));
                }

                if (GUILayout.Button("Reload config"))
                {
                    foreach (LFOThrottleData throttleData in target.GetComponentsInChildren<LFOThrottleData>())
                    {
                        throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.config.GetMaterial();
                    }
                }
            }
            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Load config"))
                {
                    var plumeconfig = LoadFromJson(path, fileName);
                    foreach (LFOThrottleData throttleData in target.GetComponentsInChildren<LFOThrottleData>())
                    {
                        int index = plumeconfig.PlumeConfigs[throttleData.transform.parent.name]
                            .FindIndex(a => a.targetGameObject == throttleData.name);
                        if (index < 0)
                            continue;
                        throttleData.config = plumeconfig.PlumeConfigs[throttleData.transform.parent.name][index];


                        throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.config.GetEditorMaterial();
                        if (UseNewShader)
                            throttleData.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("LFO/Additive");
                        throttleData.GetComponent<Renderer>().sharedMaterial.name = throttleData.name + " Plume Material";
                    }
                }
            }
        }

        void UpdateVisuals(LFOThrottleDataMasterGroup throttleGroup)
        {
            if (throttleGroup.Active)
            {
                throttleGroup.throttleDatas.ForEach(a =>
                {
                    if (a.IsVisible())
                    {
                        if (a.config == null)
                            Debug.LogWarning($"Config for {a.name} is null");
                        else
                            a.TriggerUpdateVisuals(throttleGroup.GroupThrottle / 100f, throttleGroup.GroupAtmo, 0,
                                Vector3.zero);
                    }
                });
            }
        }

        private LFOConfig LoadFromJson(string path, string fileName)
        {
            string rawJson = File.OpenText(Path.Combine(path, fileName)).ReadToEnd();

            return LFOConfig.Deserialize(rawJson);
        }

        private IEnumerator SaveToJson(LFOConfig config, string path, string fileName)
        {
            Directory.CreateDirectory(path);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = LFOConfig.Serialize(config);

            using (StreamWriter sw = File.CreateText(Path.Combine(path, fileName)))
            {
                sw.Write(json);
            }

            LFOConfig fromJson = JsonConvert.DeserializeObject<LFOConfig>(json);

            yield return null;

            AssetDatabase.Refresh();
        }
    }

    public static class Extensions
    {
        public static Material GetEditorMaterial(this PlumeConfig config)
        {
            Shader shader = Shader.Find(config.ShaderSettings.ShaderName);
            if (shader is null)
            {
                Debug.LogError($"Couldn't find shader {config.ShaderSettings.ShaderName}");
                return null;
            }

            Material material = new Material(shader);

            foreach (var kvp in config.ShaderSettings.ShaderParams)
            {
                if (kvp.Value is JObject jobject)
                {
                    if (jobject.ContainsKey("r"))
                    {
                        Color color = new Color(jobject["r"].ToObject<float>(), jobject["g"].ToObject<float>(),
                            jobject["b"].ToObject<float>(), jobject["a"].ToObject<float>());
                        material.SetColor(kvp.Key, color);
                    }
                    else if (jobject.ContainsKey("x"))
                    {
                        Vector4 vector = Vector4.zero;

                        vector.x = jobject["x"].ToObject<float>();
                        vector.y = jobject["y"].ToObject<float>();
                        if (jobject.ContainsKey("z"))
                            vector.z = jobject["z"].ToObject<float>();
                        if (jobject.ContainsKey("w"))
                            vector.w = jobject["w"].ToObject<float>();

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
                        case Single number:
                            material.SetFloat(kvp.Key, number);
                            break;
                        case double dnumber:
                            material.SetFloat(kvp.Key, (float)dnumber);
                            break;
                        case int integer:
                            material.SetFloat(kvp.Key, integer);
                            break;
                        case string textureName:
                            string path = Path.Combine("Assets", "LFO", "Noise", textureName + ".png");
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
