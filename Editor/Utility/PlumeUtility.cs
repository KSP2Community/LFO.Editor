using System.Collections.Generic;
using LFO.Shared;
using LFO.Shared.Components;
using LFO.Shared.Configs;
using UnityEngine;
using ILogger = LFO.Shared.ILogger;

namespace LFO.Editor.Utility
{
    public static class PlumeUtility
    {
        private static ILogger Logger => ServiceProvider.GetService<ILogger>();
        private static IAssetManager AssetManager => ServiceProvider.GetService<IAssetManager>();

        public static void CreatePlumeFromConfig(LFOConfig lfoConfig, GameObject parent)
        {
            var createdObjects = new Dictionary<string, GameObject>();

            foreach ((string parentName, List<PlumeConfig> plumeConfigs) in lfoConfig.PlumeConfigs)
            {
                foreach (PlumeConfig plumeConfig in plumeConfigs)
                {
                    bool isNew = FindOrCreateObject(
                        plumeConfig.TargetGameObject,
                        parentName,
                        ref createdObjects,
                        out GameObject gameObject
                    );

                    if (isNew)
                    {
                        gameObject.transform.localPosition = plumeConfig.Position;
                        gameObject.transform.localScale = plumeConfig.Scale;
                        gameObject.transform.localRotation = Quaternion.Euler(plumeConfig.Rotation);

                        var renderer = gameObject.AddComponent<MeshRenderer>();
                        var meshFilter = gameObject.AddComponent<MeshFilter>();

                        if (AssetManager.GetMesh(plumeConfig.MeshPath) is { } mesh)
                        {
                            meshFilter.sharedMesh = mesh;
                        }
                        else
                        {
                            Logger.LogError(
                                $"Couldn't find mesh {plumeConfig.MeshPath} for object {plumeConfig.TargetGameObject}"
                            );
                        }

                        var throttleData = gameObject.AddComponent<LFOThrottleData>();
                        throttleData.Config = plumeConfig;

                        renderer.sharedMaterial = throttleData.Config.GetEditorMaterial();
                        renderer.sharedMaterial.shader = AssetManager.GetShader(plumeConfig.ShaderSettings.ShaderName);

                        if (plumeConfig.ShaderSettings.ShaderName.ToLowerInvariant().Contains("volumetric"))
                        {
                            gameObject.AddComponent<LFOVolume>();
                        }
                    }
                }
            }

            var rootObjects = new List<GameObject>();
            foreach (GameObject obj in createdObjects.Values)
            {
                obj.transform.SetParent(obj.transform.parent);

                if (obj.transform.parent == null)
                {
                    rootObjects.Add(obj);
                }
            }

            foreach (GameObject rootObject in rootObjects)
            {
                var masterGroup = rootObject!.AddComponent<LFOThrottleDataMasterGroup>();
                masterGroup.GroupThrottle = 100;
                masterGroup.GroupAtmo = 1;
                rootObject!.transform.SetParent(parent != null ? parent.transform : null);
            }
        }

        /// <summary>
        /// Finds or creates a game object with the given name and parent.
        /// </summary>
        /// <param name="targetName">Name of the object to find or create.</param>
        /// <param name="parentName">Name of the parent object.</param>
        /// <param name="createdObjects">Dictionary of all created objects.</param>
        /// <param name="foundOrCreatedObject">Found or created object.</param>
        /// <returns>True if the object was created, false if it was found.</returns>
        private static bool FindOrCreateObject(
            string targetName,
            string parentName,
            ref Dictionary<string, GameObject> createdObjects,
            out GameObject foundOrCreatedObject
        )
        {
            if (createdObjects.TryGetValue(targetName, out GameObject existingObject))
            {
                foundOrCreatedObject = existingObject;

                if (parentName == null || (
                        existingObject.transform.parent != null
                        && existingObject.transform.parent.name == parentName
                    ))
                {
                    return false;
                }

                if (createdObjects.TryGetValue(parentName, out GameObject newParent))
                {
                    existingObject.transform.SetParent(newParent.transform);
                }

                return false;
            }

            var targetObject = new GameObject(targetName);

            if (parentName != null)
            {
                FindOrCreateObject(parentName, null, ref createdObjects, out GameObject parentObject);
                targetObject.transform.SetParent(parentObject.transform);
            }

            createdObjects.Add(targetName, targetObject);

            foundOrCreatedObject = targetObject;
            return true;
        }
    }
}