using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using KSP.Game;
using LuxsFlamesAndOrnaments.Settings;
using LuxsFlamesAndOrnaments;
using KSP;
using KSP.Modules;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class LFOThrottleData : KerbalMonoBehaviour, IEngineFXData
{
    public float Seed;
    public Renderer renderer;
    public Material material
    {
        get => renderer.material;
        set => renderer.material = value;
    }

    public PlumeConfig config = new();
    public List<FloatParam> FloatParams => config.FloatParams;
    public string partName = "";
    public bool IsRCS;

    public bool IsVisible()
    {
        return this.renderer != null && this.renderer.enabled;
    }

    public void ToggleVisibility(bool doTurnOn, ParticleSystemStopBehavior stopBehaviour = ParticleSystemStopBehavior.StopEmitting)
    {
        if (renderer is not null)
            renderer.enabled = doTurnOn;
    }

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        if(name.Contains("RCS"))
            IsRCS = true;
    }

    void OnEnable()
    {
        this.TriggerUpdateVisuals += this.UpdateVisuals;
    }

    private void OnDisable()
    {
        this.TriggerUpdateVisuals -= this.UpdateVisuals;
    }

    private void UpdateVisuals(float curThrottle, float curAtmo, float curAngleVel, Vector3 curAccelerationDir)
    {
        foreach (FloatParam param in FloatParams)
        {
            param.ApplyToMaterial(curThrottle, curAtmo, renderer.sharedMaterial);
        }
    }

    void OnValidate()
    {
        renderer ??= GetComponent<Renderer>();

        TriggerUpdateVisuals ??= (Action<float, float, float, Vector3>)Delegate.Combine(this.TriggerUpdateVisuals, new Action<float, float, float, Vector3>(this.UpdateVisuals));

    }
    public Action<float, float, float, Vector3> TriggerUpdateVisuals { get; set; }
}
