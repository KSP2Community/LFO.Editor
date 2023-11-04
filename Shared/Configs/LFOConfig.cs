using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LFO.Shared.Configs
{
    [Serializable]
    public class LFOConfig
    {
        public string PartName;
        public Dictionary<string, List<PlumeConfig>> PlumeConfigs = new();

        public static JsonSerializerSettings SerializerSettings = new()
        {
            Converters = new JsonConverter[] { new Vec4Conv(), new Vec3Conv(), new Vec2Conv(), new ColorConv() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static string Serialize(LFOConfig config)
        {
            return JsonConvert.SerializeObject(config, SerializerSettings);
        }

        public static LFOConfig Deserialize(string rawJson)
        {
            return JsonConvert.DeserializeObject<LFOConfig>(rawJson, SerializerSettings);
        }
    }
}