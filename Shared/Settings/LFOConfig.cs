using System;
using System.Collections.Generic;
using System.Linq;
using KSP.VFX;
using LFO.Shared.Components;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LFO.Shared.Settings
{
    [Serializable]
    public class LFOConfig
    {
        public string PartName;
        public Dictionary<string, List<PlumeConfig>> PlumeConfigs;

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

        internal void InstantiatePlume(string partName, ref GameObject prefab)
        {
            var vfxManager = prefab.GetComponent<ThrottleVFXManager>();
            var effects = new List<ThrottleVFXManager.EngineEffect>();
            var deletedObjects = new List<string>();

            foreach (KeyValuePair<string, List<PlumeConfig>> kvp in PlumeConfigs)
            {
                Transform tParent = prefab.transform.FindChildRecursive(kvp.Key);
                if (tParent == null)
                {
                    string sanitizedKey = "[LFO] " + kvp.Key + " [vfx_exh]";
                    tParent = prefab.transform.FindChildRecursive(sanitizedKey);
                    if (tParent == null)
                    {
                        Debug.LogWarning(
                            $"Couldn't find GameObject named {kvp.Key} to be set as parent. Trying to create under thrustTransform"
                        );
                        Transform tTransform = prefab.transform.FindChildRecursive("thrustTransform");
                        if (tTransform == null)
                        {
                            throw new NullReferenceException(
                                "Couldn't find GameObject named thrustTransform to enforce plume creation"
                            );
                        }

                        tParent = new GameObject(kvp.Key).transform;
                        tParent.SetParent(prefab.transform.FindChildRecursive("thrustTransform"));
                        tParent.localRotation = Quaternion.Euler(270, 0, 0);
                        tParent.localPosition = Vector3.zero;
                        tParent.localScale = Vector3.one;
                    }
                }

                if (tParent == null)
                {
                    continue;
                }

                int childCount = tParent.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    deletedObjects.Add(
                        prefab.transform
                            .FindChildRecursive(tParent.name)
                            .GetChild(i)
                            .gameObject
                            .name
                    );
                    prefab.transform
                        .FindChildRecursive(tParent.name)
                        .GetChild(i)
                        .gameObject
                        .DestroyGameObjectImmediate(); //Cleanup of every other plume (could be more efficient)
                    //TODO: Add way to avoid certain cleanups (particles etc)
                }

                foreach (PlumeConfig config in kvp.Value)
                {
                    try
                    {
                        var plume = new GameObject(
                            "[LFO] " + config.TargetGameObject + " [vfx_exh]",
                            typeof(MeshRenderer),
                            typeof(MeshFilter),
                            typeof(LFOThrottleData)
                        );
                        var throttleData = plume.GetComponent<LFOThrottleData>();
                        throttleData.PartName = partName;
                        throttleData.Material = config.GetMaterial();
                        bool volumetric = false;

                        if (throttleData.Material.shader.name.ToLower().Contains("volumetric"))
                        {
                            volumetric = true;
                            plume.AddComponent<LFOVolume>();
                        }

                        var renderer = plume.GetComponent<MeshRenderer>();
                        var filter = plume.GetComponent<MeshFilter>();

                        if (volumetric)
                        {
                            var gameObject = GameObject.CreatePrimitive(
                                config.MeshPath.ToLower() == "cylinder"
                                    ? PrimitiveType.Cylinder
                                    : PrimitiveType.Cube
                            );

                            filter.mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
                            Object.Destroy(gameObject);
                        }
                        else
                        {
                            LFO.TryGetMesh(config.MeshPath, out Mesh mesh);
                            if (mesh != null)
                            {
                                filter.mesh = mesh;
                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"Couldn't find mesh at {config.MeshPath} for {config.TargetGameObject}"
                                );
                            }
                        }

                        LFO.RegisterPlumeConfig(partName, plume.name, config);

                        plume.transform.parent = prefab.transform.FindChildRecursive(tParent.name).transform;
                        plume.layer = 1; //TransparentFX layer

                        plume.transform.localPosition = config.Position;
                        plume.transform.localRotation = Quaternion.Euler(config.Rotation);
                        plume.transform.localScale = config.Scale;
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        renderer.enabled = false;

                        effects.Add(new ThrottleVFXManager.EngineEffect
                        {
                            EffectReference = plume
                        });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{config.TargetGameObject} was not created! \tException:\n{e}");
                    }
                }
            }

            vfxManager.FXModeActionEvents ??= new ThrottleVFXManager.FXModeActionEvent[]
            {
                new()
                {
                    EngineModeIndex = 0,
                    ActionEvents = new ThrottleVFXManager.FXActionEvent[]
                    {
                        new()
                        {
                            ModeEvent = ThrottleVFXManager.FXmodeEvent.FXModeRunning,
                            EngineEffects = effects.ToArray()
                        }
                    }
                }
            };

            if (vfxManager.FXModeActionEvents.Length == 0)
            {
                vfxManager.FXModeActionEvents =
                    vfxManager.FXModeActionEvents.AddItem(new ThrottleVFXManager.FXModeActionEvent()).ToArray();
            }

            ThrottleVFXManager.FXModeActionEvent firstEngineMode = vfxManager.FXModeActionEvents[0];

            firstEngineMode.ActionEvents ??= Array.Empty<ThrottleVFXManager.FXActionEvent>();

            ThrottleVFXManager.FXActionEvent runningActionEvent = firstEngineMode.ActionEvents.FirstOrDefault(
                a => a.ModeEvent == ThrottleVFXManager.FXmodeEvent.FXModeRunning
            );
            if (runningActionEvent is null)
            {
                firstEngineMode.ActionEvents = firstEngineMode.ActionEvents.AddItem(new ThrottleVFXManager.FXActionEvent
                {
                    ModeEvent = ThrottleVFXManager.FXmodeEvent.FXModeRunning,
                    EngineEffects = Array.Empty<ThrottleVFXManager.EngineEffect>()
                }).ToArray();
                runningActionEvent = firstEngineMode.ActionEvents.FirstOrDefault(
                    a => a.ModeEvent == ThrottleVFXManager.FXmodeEvent.FXModeRunning
                );
            }

            runningActionEvent!.EngineEffects ??= Array.Empty<ThrottleVFXManager.EngineEffect>();

            runningActionEvent.EngineEffects = runningActionEvent.EngineEffects.AddRange(effects.ToArray());
        }
    }
}