using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LFO.Shared.Configs
{
    [Serializable]
    public struct ShaderConfig
    {
        [JsonRequired] public string ShaderName;
        [JsonRequired] public Dictionary<string, object> ShaderParams;

        private static readonly IAssetManager AssetManager;
        private static readonly ILogger Logger;

        static ShaderConfig()
        {
            Logger = ServiceProvider.GetService<ILogger>();
            AssetManager = ServiceProvider.GetService<IAssetManager>();
        }

        internal void Add(string paramName, object value)
        {
            ShaderParams.Add(paramName, value);
        }

        public Material ToMaterial()
        {
            var shader = AssetManager.GetAsset<Material>(ShaderName).shader;
            if (shader == null)
            {
                Logger.LogError($"Couldn't find shader {ShaderName}");
                return null;
            }

            var material = new Material(shader);

            foreach ((string property, object value) in ShaderParams)
            {
                Debug.Log($"[LFO] Setting {property} to {value} on {material.name}");
                switch (value)
                {
                    case Color color:
                        material.SetColor(property, color);
                        break;
                    case Vector2 vector2:
                        material.SetVector(property, vector2);
                        break;
                    case Vector3 vector3:
                        material.SetVector(property, vector3);
                        break;
                    case Vector4 vector4:
                        material.SetVector(property, vector4);
                        break;
                    case float number:
                        material.SetFloat(property, number);
                        break;
                    case int integer:
                        material.SetFloat(property, integer);
                        break;
                    case string textureName:
                        AssignTexture(material, property, textureName);
                        break;
                }
            }

            return material;
        }

        private void AssignTexture(Material material, string property, string textureName)
        {
#if !UNITY_EDITOR
            try
            {
                if (AssetManager.GetAsset<Texture>(textureName) is not { } texture)
                {
                    throw new NullReferenceException(
                        $"[LFO] Couldn't find texture with name {textureName}. Make sure the textures have the right name!"
                    );
                }

                Logger.LogDebug(
                    $"Assigning texture {textureName} to property {property} of {material.name}"
                );
                material.SetTexture(property, texture);
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Error assigning texture {textureName} to property {property} of {material.name}: {e}"
                );
            }
#endif
        }

        public static ShaderConfig GenerateConfig(Material material)
        {
            Shader shader = material.shader;
            var config = new ShaderConfig
            {
                ShaderName = shader.name,
                ShaderParams = new Dictionary<string, object>()
            };

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string paramName = shader.GetPropertyName(i);
                ShaderPropertyType paramType = shader.GetPropertyType(i);

                switch (paramType)
                {
                    case ShaderPropertyType.Color:
                    {
                        Color defaultColor = shader.GetPropertyDefaultVectorValue(i);
                        Color setColor = material.GetColor(paramName);

                        if (setColor != defaultColor)
                        {
                            config.Add(paramName, setColor);
                        }

                        break;
                    }
                    case ShaderPropertyType.Vector:
                    {
                        Vector4 defaultVector = shader.GetPropertyDefaultVectorValue(i);
                        Vector4 setVector = material.GetVector(paramName);

                        if (setVector != defaultVector)
                        {
                            config.Add(paramName, setVector);
                        }

                        break;
                    }
                    case ShaderPropertyType.Float:
                    {
                        float defaultFloat = shader.GetPropertyDefaultFloatValue(i);
                        float setFloat = material.GetFloat(paramName);

                        if (setFloat != defaultFloat)
                        {
                            config.Add(paramName, setFloat);
                        }

                        break;
                    }
                    case ShaderPropertyType.Range:
                    {
                        Vector2 defaultRange = shader.GetPropertyRangeLimits(i);
                        float defaultValue = shader.GetPropertyDefaultFloatValue(i);
                        float setRange = material.GetFloat(paramName);

                        if (setRange != defaultValue)
                        {
                            config.Add(paramName, setRange);
                        }

                        break;
                    }
                    case ShaderPropertyType.Texture:
                    {
                        string defaultTexture = shader.GetPropertyTextureDefaultName(i);
                        string setTexture = material.GetTexture(paramName).name;

                        if (setTexture != defaultTexture)
                        {
                            config.Add(paramName, setTexture);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(paramType), paramType, null);
                }
            }

            return config;
        }

        public override string ToString()
        {
            return $"{ShaderName} ({ShaderParams.Count} changes)";
        }

        public static ShaderConfig Default => new()
        {
            ShaderName = "LFOAdditive"
        };

        [JsonConverter(typeof(ShaderConfig))]
        public class ShaderConfigJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ShaderConfig);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer
            )
            {
                var jObject = JObject.Load(reader);
                var toReturn = new ShaderConfig
                {
                    ShaderName = (string)jObject["ShaderName"],
                    ShaderParams = new Dictionary<string, object>()
                };

                foreach (JToken element in jObject["ShaderParams"]!.Children())
                {
                    if (element is JProperty property)
                    {
                        string paramName = property.Name;
                        JToken value = property.Value;

                        switch (value.Type)
                        {
                            case JTokenType.Object:

                                JToken xToken = value["x"];
                                JToken rToken = value["r"];

                                if (xToken != null)
                                {
                                    var vector = new Vector4
                                    {
                                        x = (float)value["x"],
                                        y = (float)value["y"],
                                        z = (float)value["z"],
                                        w = (float)value["w"]
                                    };

                                    toReturn.Add(paramName, vector);
                                }
                                else if (rToken is not null)
                                {
                                    var color = new Color
                                    {
                                        r = (float)value["r"],
                                        g = (float)value["g"],
                                        b = (float)value["b"],
                                        a = (float)value["a"]
                                    };

                                    toReturn.Add(paramName, color);
                                }

                                break;
                            case JTokenType.Integer:
                            case JTokenType.Float:
                                toReturn.Add(paramName, value.ToObject<float>());
                                break;
                            case JTokenType.String:
                                toReturn.Add(paramName, value.ToString());
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(value.Type), value.Type, null);
                        }
                    }
                }

                //serializer.Populate(jObject.CreateReader(), toReturn);

                return toReturn;
            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
            }
        }
    }
}