﻿using System;
using System.Collections.Generic;
using LFO.Shared.Settings;
using UnityEngine;

namespace LFO.Shared
{
    public class LFO
    {
        public const string ResourcesPath = "lfo/lfo-resources/packages/lfo.editor/assets/";
        public const string MeshesPath = ResourcesPath + "meshes/";
        public const string NoisesPath = ResourcesPath + "noise/";
        public const string TexturesPath = ResourcesPath + "textures/";
        public const string ProfilesPath = ResourcesPath + "profiles/";
        public const string ShadersPath = ResourcesPath + "shaders/";

        public static LFO Instance => _instance ??= new LFO();

        private static LFO _instance;

        public readonly Dictionary<string, LFOConfig> PartNameToConfigDict = new();

        public readonly Dictionary<string, Dictionary<string, PlumeConfig>> GameObjectToPlumeDict =
            new();

        public readonly Dictionary<string, Shader> LoadedShaders = new();

        public static void RegisterLFOConfig(string partName, LFOConfig config)
        {
            Instance.PartNameToConfigDict.TryAdd(partName, new LFOConfig());

            if (!Instance.GameObjectToPlumeDict.ContainsKey(partName))
            {
                Instance.GameObjectToPlumeDict.Add(partName, new Dictionary<string, PlumeConfig>());
            }

            Instance.PartNameToConfigDict[partName] = config;
        }

        public static bool TryGetConfig(string partName, out LFOConfig config)
        {
            if (Instance.PartNameToConfigDict.ContainsKey(partName))
            {
                config = Instance.PartNameToConfigDict[partName];
                return true;
            }

            config = null;
            return false;
        }

        public static bool TryGetMesh(string meshPath, out Mesh mesh)
        {
#if !UNITY_EDITOR

            mesh = null;
            if (SpaceWarp.API.Assets.AssetManager.TryGetAsset(
                    MeshesPath + meshPath.ToLower() + ".fbx",
                    out GameObject fbxPrefab
                ))
            {
                mesh = fbxPrefab.TryGetComponent(out SkinnedMeshRenderer skinnedRenderer)
                    ? skinnedRenderer.sharedMesh
                    : fbxPrefab.GetComponent<MeshFilter>().mesh;
                return true;
            }

            if (!SpaceWarp.API.Assets.AssetManager.TryGetAsset(
                    MeshesPath + meshPath.ToLower().Remove(meshPath.Length - 2) + ".obj",
                    out GameObject objPrefab
                )) //obj's meshes are named as "meshName_#" with # being the meshID
            {
                return false;
            }

            if (objPrefab.GetComponentInChildren<MeshFilter>() == null)
            {
                return false;
            }

            mesh = objPrefab.GetComponentInChildren<MeshFilter>().mesh;
            return true;

#else
            mesh = null;
            return true;
#endif
        }

        public static Shader GetShader(string name)
        {
            if (Instance.LoadedShaders.ContainsKey(name))
            {
                return Instance.LoadedShaders[name];
            }

            throw new IndexOutOfRangeException(
                $"[LFO] Shader {name} is not present in the internal shader collection. Check logs for more information."
            );
        }

        internal static void RegisterPlumeConfig(string partName, string id, PlumeConfig config)
        {
            if (Instance.GameObjectToPlumeDict.ContainsKey(partName))
            {
                if (Instance.GameObjectToPlumeDict[partName].ContainsKey(id))
                {
                    Instance.GameObjectToPlumeDict[partName][id] = config;
                }
                else
                {
                    Instance.GameObjectToPlumeDict[partName].Add(id, config);
                }
            }
            else
            {
                Debug.LogWarning($"{partName} has no registered plume");
            }
        }

        internal static bool TryGetPlumeConfig(string partName, string id, out PlumeConfig config)
        {
            if (Instance.GameObjectToPlumeDict.ContainsKey(partName))
            {
                return Instance.GameObjectToPlumeDict[partName].TryGetValue(id, out config);
            }

            Debug.LogWarning($"{partName} has no registered plume");
            config = null;
            return false;
        }
    }
}