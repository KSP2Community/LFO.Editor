using System;
using System.Collections.Generic;
using LFO.Shared.Components;
using LFO.Shared.ShaderEditor;
using Newtonsoft.Json;
using UnityEngine;

namespace LFO.Shared.Settings
{
    [Serializable]
    public class PlumeConfig
    {
        public string MeshPath;
        public string TargetGameObject; //Name that the gameObject will have
        public ShaderConfig ShaderSettings;
        public Vector3 Position;
        public Vector3 Scale = Vector3.one;
        public Vector3 Rotation;
        public List<FloatParam> FloatParams;

        public static string Serialize(List<PlumeConfig> config)
        {
            return JsonConvert.SerializeObject(config, Formatting.Indented);
        }

        public static List<PlumeConfig> Deserialize(string rawJson)
        {
            return JsonConvert.DeserializeObject<List<PlumeConfig>>(rawJson);
        }

        public static PlumeConfig CreateConfig(LfoThrottleData data)
        {
            Transform transform = data.transform;
            return new PlumeConfig
            {
                MeshPath = data.GetComponent<MeshFilter>().mesh.name,
                Position = transform.localPosition,
                Rotation = transform.localRotation.eulerAngles,
                Scale = transform.localScale,
                FloatParams = data.FloatParams
            };
        }

        public Material GetMaterial()
        {
            Material material = ShaderSettings.ToMaterial();
            return material;
        }

        public override string ToString()
        {
            return $"{TargetGameObject} - {MeshPath}";
        }
    }
}
