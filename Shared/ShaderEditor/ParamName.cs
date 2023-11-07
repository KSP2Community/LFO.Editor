using System;

namespace LFO.Shared.ShaderEditor
{
    [Serializable]
    public class ParamName
    {
        public string Value;

        public ParamName()
        {
        }

        public ParamName(string value)
        {
            Value = value;
        }

        public static implicit operator string(ParamName paramName)
        {
            return paramName.Value;
        }

        public static implicit operator ParamName(string paramName)
        {
            return new ParamName { Value = paramName };
        }

        public override string ToString()
        {
            return Value;
        }
    }
}