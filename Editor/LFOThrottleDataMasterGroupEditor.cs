using KSP;
using LFO.Shared.Components;
using LFO.Shared.Configs;
using Newtonsoft.Json.Linq;
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
            var group = (LFOThrottleDataMasterGroup)target;

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
            var path = $"{Application.dataPath}/plugin_template/assets/plumes/";
            var filename = partData != null ? $"{partData.Data.partName}.json" : $"{group.name}.json";

            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Save config"))
                {
                    HandleSaveConfig(group, partData, path, filename);
                }

                if (GUILayout.Button("Reload config"))
                {
                    HandleReloadConfig(group);
                }
            }
            EditorGUILayout.Space(5);
            {
                if (GUILayout.Button("Load config"))
                {
                    HandleLoadConfig(group, path, filename);
                }
            }
        }

        private static void HandleSaveConfig(
            LFOThrottleDataMasterGroup group,
            CorePartData partData,
            string path,
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

            group.StartCoroutine(SaveToJson(config, path, filename));
        }

        private static void HandleReloadConfig(LFOThrottleDataMasterGroup group)
        {
            foreach (var throttleData in group.GetComponentsInChildren<LFOThrottleData>())
            {
                throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.Config.GetMaterial();
            }
        }

        private void HandleLoadConfig(LFOThrottleDataMasterGroup group, string path, string filename)
        {
            var plumeConfig = LoadFromJson(path, filename);
            foreach (var throttleData in group.GetComponentsInChildren<LFOThrottleData>())
            {
                int index = plumeConfig.PlumeConfigs[throttleData.transform.parent.name]
                    .FindIndex(a => a.TargetGameObject == throttleData.name);
                if (index < 0)
                {
                    continue;
                }

                throttleData.Config = plumeConfig.PlumeConfigs[throttleData.transform.parent.name][index];

                throttleData.GetComponent<Renderer>().sharedMaterial = throttleData.Config.GetEditorMaterial();
                if (UseNewShader)
                {
                    throttleData.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("LFO/Additive");
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
                    Debug.LogWarning($"Config for {throttleData.name} is null");
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

        private static LFOConfig LoadFromJson(string path, string fileName)
        {
            string rawJson = File.OpenText(Path.Combine(path, fileName)).ReadToEnd();

            return LFOConfig.Deserialize(rawJson);
        }

        private static IEnumerator SaveToJson(LFOConfig config, string path, string fileName)
        {
            Directory.CreateDirectory(path);

            // var settings = new JsonSerializerSettings
            // {
            //     ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            // };

            string json = LFOConfig.Serialize(config);

            using (StreamWriter sw = File.CreateText(Path.Combine(path, fileName)))
            {
                sw.Write(json);
            }

            // var fromJson = JsonConvert.DeserializeObject<LFOConfig>(json);

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

            foreach ((string param, object value) in config.ShaderSettings.ShaderParams)
            {
                if (value is JObject jobject)
                {
                    SetJObjectParam(jobject, material, param);
                }
                else
                {
                    SetOtherParam(value, material, param);
                }
            }

            return material;
        }

        private static void SetJObjectParam(JObject jobject, Material material, string param)
        {
            if (jobject.ContainsKey("r"))
            {
                var color = new Color(
                    jobject["r"].ToObject<float>(),
                    jobject["g"].ToObject<float>(),
                    jobject["b"].ToObject<float>(),
                    jobject["a"].ToObject<float>()
                );
                material.SetColor(param, color);
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

                material.SetVector(param, vector);
            }
        }

        private static void SetOtherParam(object value, Material material, string param)
        {
            switch (value)
            {
                case Color color:
                    material.SetColor(param, color);
                    break;
                case Vector2 vector2:
                    material.SetVector(param, vector2);
                    break;
                case Vector3 vector3:
                    material.SetVector(param, vector3);
                    break;
                case Vector4 vector4:
                    material.SetVector(param, vector4);
                    break;
                case float number:
                    material.SetFloat(param, number);
                    break;
                case double dnumber:
                    material.SetFloat(param, (float)dnumber);
                    break;
                case int integer:
                    material.SetFloat(param, integer);
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
                    material.SetTexture(param, AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                    break;
            }
        }
    }
}