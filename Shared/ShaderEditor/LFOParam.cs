using System;
using UnityEngine;

namespace LFO.Shared.ShaderEditor
{
    [Serializable]
    public abstract class LFOParam
    {
        [HideInInspector] public int ParamHash = -1;
        [HideInInspector] public bool IsDirty;

        public string ParamName;
        public bool UseAtmoCurve;
        public CurveType AtmoCurveType;
        public AnimationCurve AtmoMultiplierCurve;
        public bool UseThrottleCurve;
        public CurveType ThrottleCurveType;
        public AnimationCurve ThrottleMultiplierCurve;

        public abstract void ApplyToMaterial(float curThrottle, float curAtmo, Material material);
    }
}