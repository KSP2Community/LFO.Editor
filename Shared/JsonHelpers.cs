using UnityEngine;
using Newtonsoft.Json;
using System;
using LFO.Shared.ShaderEditor;
using Newtonsoft.Json.Linq;

namespace LFO.Shared
{
    public class Vec4Conv : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector4);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var jObject = JObject.Load(reader);
            var toReturn = new Vector4
            {
                x = jObject["x"]!.ToObject<float>(),
                y = jObject["y"]!.ToObject<float>(),
                z = jObject["z"]!.ToObject<float>(),
                w = jObject["w"]!.ToObject<float>()
            };

            serializer.Populate(jObject.CreateReader(), toReturn);

            return toReturn;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector4)value!;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(v.x);
            writer.WritePropertyName("y");
            writer.WriteValue(v.y);
            writer.WritePropertyName("z");
            writer.WriteValue(v.z);
            writer.WritePropertyName("w");
            writer.WriteValue(v.w);
            writer.WriteEndObject();
        }
    }

    public class Vec3Conv : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector3);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var jObject = JObject.Load(reader);
            var toReturn = new Vector3
            {
                x = jObject["x"]!.ToObject<float>(),
                y = jObject["y"]!.ToObject<float>(),
                z = jObject["z"]!.ToObject<float>()
            };

            serializer.Populate(jObject.CreateReader(), toReturn);

            return toReturn;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector3)value!;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(v.x);
            writer.WritePropertyName("y");
            writer.WriteValue(v.y);
            writer.WritePropertyName("z");
            writer.WriteValue(v.z);
            writer.WriteEndObject();
        }
    }

    public class Vec2Conv : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Vector2);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var jObject = JObject.Load(reader);
            var toReturn = new Vector2
            {
                x = jObject["x"]!.ToObject<float>(),
                y = jObject["y"]!.ToObject<float>()
            };

            serializer.Populate(jObject.CreateReader(), toReturn);

            return toReturn;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vector2)value!;

            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(v.x);
            writer.WritePropertyName("y");
            writer.WriteValue(v.y);
            writer.WriteEndObject();
        }
    }

    public class ColorConv : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var jObject = JObject.Load(reader);
            var toReturn = new Color
            {
                r = jObject["r"]!.ToObject<float>(),
                g = jObject["g"]!.ToObject<float>(),
                b = jObject["b"]!.ToObject<float>(),
                a = jObject["a"]!.ToObject<float>()
            };

            serializer.Populate(jObject.CreateReader(), toReturn);

            return toReturn;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Color)value!;

            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(v.r);
            writer.WritePropertyName("g");
            writer.WriteValue(v.g);
            writer.WritePropertyName("b");
            writer.WriteValue(v.b);
            writer.WritePropertyName("a");
            writer.WriteValue(v.a);
            writer.WriteEndObject();
        }
    }

    public class ParamNameConverter  : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ParamName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var stringValue = (string)reader.Value!;
            return new ParamName(stringValue);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var paramNameValue = (ParamName)value!;
            writer.WriteValue(paramNameValue.Value);
        }
    }
}