using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Game;
using LFO.Shared.ShaderEditor;

namespace LFO.Shared.Components
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Renderer))]
    public class LFOThrottleData : KerbalMonoBehaviour, IEngineFXData
    {
        public float Seed;
        public Renderer Renderer;
        public Settings.PlumeConfig Config = new();
        public string PartName = "";
        public bool IsRcs;

        public Material Material
        {
            get => Renderer.material;
            set => Renderer.material = value;
        }

        public List<FloatParam> FloatParams => Config.FloatParams;

        public Action<float, float, float, Vector3> TriggerUpdateVisuals { get; set; }

        public bool IsVisible()
        {
            return Renderer != null && Renderer.enabled;
        }

        public void ToggleVisibility(
            bool doTurnOn,
            ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmitting
        )
        {
            if (Renderer is not null)
            {
                Renderer.enabled = doTurnOn;
            }
        }

        private void Awake()
        {
            Renderer = GetComponent<Renderer>();
            if (name.Contains("RCS"))
            {
                IsRcs = true;
            }
        }

        private void Start()
        {
            //var engineComp = this.getcomponentpar<Module_Engine>(true);
            //string partName = (engineComp.PartBackingMode == KSP.Sim.Definitions.PartBehaviourModule.PartBackingModes.OAB) ? engineComp.OABPart.PartName : engineComp.part.name;

            if (!IsRcs && (string.IsNullOrEmpty(PartName) || !LFO.TryGetPlumeConfig(PartName, name, out Config)))
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            TriggerUpdateVisuals += UpdateVisuals;
        }

        private void OnDisable()
        {
            TriggerUpdateVisuals -= UpdateVisuals;
        }

        private void UpdateVisuals(float curThrottle, float curAtmo, float curAngleVel, Vector3 curAccelerationDir)
        {
            foreach (FloatParam param in FloatParams)
            {
                param.ApplyToMaterial(curThrottle, curAtmo, Renderer.sharedMaterial);
            }
        }

        private void OnValidate()
        {
            if (Renderer == null)
            {
                Renderer = GetComponent<Renderer>();
            }

            TriggerUpdateVisuals ??= UpdateVisuals;
        }
    }
}