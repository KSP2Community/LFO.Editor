using KSP.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LFO.Shared.Components
{
    [ExecuteInEditMode]
    public class LfoThrottleDataMasterGroup : KerbalMonoBehaviour, IEngineFXData
    {
        public Action<float, float, float, Vector3> TriggerUpdateVisuals { get; set; }
        private List<IEngineFXData> Children => ThrottleDatas.Select(a => (IEngineFXData)a).ToList();
        public List<LfoThrottleData> ThrottleDatas = new();

        public bool OverrideControls;
        public bool Active;
        [Range(0, 100f)] public float GroupThrottle;
        [Range(0, 1.1f)] public float GroupAtmo;

        private float _oldThrottle = -1, _oldAtmo = -1;
        private System.Random _rng;

        public bool IsVisible()
        {
            return Children.All(child => child.IsVisible());
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                Active = true;
            }

            ThrottleDatas = GetComponentsInChildren<LfoThrottleData>(true).ToList();
            _rng = new System.Random(gameObject.GetHashCode());
            NewSeedForAll();
        }

        private void NewSeedForAll()
        {
            ThrottleDatas.ForEach(a => a.Seed = (float)_rng.NextDouble());
        }

        public void ToggleVisibility(
            bool doTurnOn,
            ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmitting
        )
        {
            ThrottleDatas.ForEach(a => a.ToggleVisibility(doTurnOn));
        }

        private void UpdateVisuals(float curThrottle, float curAtmo, float curAngleVel, Vector3 curAccelerationDir)
        {
            ThrottleDatas.ForEach(a => a.TriggerUpdateVisuals(curThrottle, curAtmo, curAngleVel, curAccelerationDir));
        }

        private void OnEnable()
        {
            TriggerUpdateVisuals += UpdateVisuals;
        }

        private void OnDisable()
        {
            TriggerUpdateVisuals -= UpdateVisuals;
        }

        private void Update()
        {
            if (Application.isEditor && (GroupThrottle != _oldThrottle || GroupAtmo != _oldAtmo))
            {
                UpdateVisuals(GroupThrottle / 100f, GroupAtmo, 0, Vector3.zero);
                _oldAtmo = GroupAtmo;
                _oldThrottle = GroupThrottle;
            }
        }
    }
}