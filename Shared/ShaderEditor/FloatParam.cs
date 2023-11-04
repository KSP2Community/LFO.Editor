using System;
using UnityEngine;

namespace LFO.Shared.ShaderEditor
{
    [Serializable]
    public class FloatParam : LFOParam
    {
        public float Value = float.MinValue;

        public override void ApplyToMaterial(float curThrottle, float curAtmo, Material material)
        {
            float calculatedValue = Value;

            if (UseAtmoCurve)
            {
                calculatedValue = EvaluateAndApplyCurve(
                    AtmoMultiplierCurve,
                    AtmoCurveType,
                    calculatedValue,
                    curAtmo
                );
            }

            if (UseThrottleCurve)
            {
                calculatedValue = EvaluateAndApplyCurve(
                    ThrottleMultiplierCurve,
                    ThrottleCurveType,
                    calculatedValue,
                    curThrottle
                );
            }

            if (ParamHash == 0)
            {
                ParamHash = Shader.PropertyToID(ParamName);
            }

            material.SetFloat(ParamName, calculatedValue);
        }

        private static float EvaluateAndApplyCurve(
            AnimationCurve curve,
            CurveType curveType,
            float value,
            float parameter
        )
        {
            float evaluated = curve.Evaluate(parameter);

            switch (curveType)
            {
                case CurveType.Base:
                    value = evaluated;
                    break;
                case CurveType.Multiply:
                    value *= evaluated;
                    break;
                case CurveType.Add:
                    value += evaluated;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curveType), curveType, null);
            }

            return value;
        }
    }
}