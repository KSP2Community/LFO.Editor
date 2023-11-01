using System;
using System.Collections.Generic;
using System.Linq;
using KSP.VFX;
using LFO.Shared.Components;
using Newtonsoft.Json;
using UnityEngine;
using static KSP.VFX.ThrottleVFXManager;
using Object = UnityEngine.Object;

namespace LFO.Shared.Settings
{
    [Serializable]
    public class LfoConfig
    {
        public static JsonSerializerSettings SerializerSettings = new()
        {
            Converters = new JsonConverter[] { new Vec4Conv(), new Vec3Conv(), new Vec2Conv(), new ColorConv() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public string PartName;
        public Dictionary<string, List<PlumeConfig>> PlumeConfigs;

        public static string Serialize(LfoConfig config)
        {
            return JsonConvert.SerializeObject(config, SerializerSettings);
        }

        public static LfoConfig Deserialize(string rawJson)
        {
            return JsonConvert.DeserializeObject<LfoConfig>(rawJson, SerializerSettings);
        }

        internal void InstantiatePlume(string partName, ref GameObject prefab)
        {
            var vfxManager = prefab.GetComponent<ThrottleVFXManager>();
            var effects = new List<EngineEffect>();
            var deletedObjects = new List<string>();

            foreach (var kvp in PlumeConfigs)
            {
                Transform tParent = prefab.transform.FindChildRecursive(kvp.Key);
                if (tParent is null)
                {
                    string sanitizedKey = "[LFO] " + kvp.Key + " [vfx_exh]";
                    tParent = prefab.transform.FindChildRecursive(sanitizedKey);
                    if (tParent is null)
                    {
                        Debug.LogWarning(
                            $"Couldn't find GameObject named {kvp.Key} to be set as parent. Trying to create under thrustTransform"
                        );
                        var tTransform = prefab.transform.FindChildRecursive("thrustTransform");
                        if (tTransform is null)
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

                GameObject parent = tParent.gameObject;
                int childCount = tParent.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    deletedObjects.Add(prefab.transform.FindChildRecursive(tParent.name).GetChild(i).gameObject.name);
                    prefab.transform.FindChildRecursive(tParent.name).GetChild(i).gameObject
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
                            typeof(LfoThrottleData)
                        );
                        var throttleData = plume.GetComponent<LfoThrottleData>();
                        throttleData.PartName = partName;
                        throttleData.Material = config.GetMaterial();
                        bool volumetric = false;

                        if (throttleData.Material.shader.name.ToLower().Contains("volumetric"))
                        {
                            volumetric = true;
                            plume.AddComponent<LfoVolume>();
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
                            if (mesh is not null)
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

                        effects.Add(new EngineEffect() { EffectReference = plume });
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{config.TargetGameObject} was not created! \tException:\n{e}");
                    }
                }
            }

            vfxManager.FXModeActionEvents ??= new FXModeActionEvent[]
            {
                new()
                {
                    EngineModeIndex = 0,
                    ActionEvents = new FXActionEvent[]
                    {
                        new()
                        {
                            ModeEvent = FXmodeEvent.FXModeRunning,
                            EngineEffects = effects.ToArray()
                        }
                    }
                }
            };

            if (vfxManager.FXModeActionEvents.Length == 0)
            {
                vfxManager.FXModeActionEvents =
                    vfxManager.FXModeActionEvents.AddItem(new FXModeActionEvent()).ToArray();
            }

            var firstEngineMode = vfxManager.FXModeActionEvents[0];

            firstEngineMode.ActionEvents ??= Array.Empty<FXActionEvent>();

            var runningActionEvent = firstEngineMode.ActionEvents.FirstOrDefault(
                a => a.ModeEvent == FXmodeEvent.FXModeRunning
            );
            if (runningActionEvent is null)
            {
                firstEngineMode.ActionEvents = firstEngineMode.ActionEvents.AddItem(new FXActionEvent
                {
                    ModeEvent = FXmodeEvent.FXModeRunning,
                    EngineEffects = Array.Empty<EngineEffect>()
                }).ToArray();
                runningActionEvent = firstEngineMode.ActionEvents.FirstOrDefault(
                    a => a.ModeEvent == FXmodeEvent.FXModeRunning
                );
            }

            runningActionEvent!.EngineEffects ??= Array.Empty<EngineEffect>();

            runningActionEvent.EngineEffects = runningActionEvent.EngineEffects.AddRange(effects.ToArray());
        }
    }
}