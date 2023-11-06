using System;
using LFO.Shared;
using UnityEditor;
using UnityEngine;
using ILogger = LFO.Shared.ILogger;

namespace LFO.Editor.Services
{
    public class UnityAssetManager : BaseAssetManager
    {
        private static ILogger Logger => ServiceProvider.GetService<ILogger>();
        private static readonly string[] AssetFolders = { "Assets", "Packages/lfo.editor/Assets" };

        public override T GetAsset<T>(string name)
        {
            string[] foundGuids = AssetDatabase.FindAssets(name, AssetFolders);
            if (foundGuids.Length == 0)
            {
                if (GetRenamedAssetName(name) != null)
                {
                    return GetAsset<T>(GetRenamedAssetName(name));
                }

                throw new Exception($"No asset found with name {name}.");
            }

            T foundAsset = null;
            foreach (string guid in foundGuids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null)
                {
                    continue;
                }

                if (foundAsset != null)
                {
                    throw new Exception($"Multiple assets found with name {name}.");
                }

                foundAsset = asset;
            }

            if (foundAsset == null)
            {
                throw new Exception($"No asset with type {typeof(T).Name} found for the name {name}.");
            }

            return foundAsset;
        }

        public override bool TryGetAsset<T>(string name, out T asset)
        {
            asset = null;

            string[] foundGuids = AssetDatabase.FindAssets(name, AssetFolders);
            if (foundGuids.Length == 0)
            {
                if (GetRenamedAssetName(name) is { } renamedAssetName)
                {
                    return TryGetAsset(renamedAssetName, out asset);
                }

                Debug.LogWarning($"No asset found with name {name}.");
                return false;

            }

            T foundAsset = null;
            foreach (string guid in foundGuids)
            {
                if (AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)) is not { } assetAtPath)
                {
                    continue;
                }

                if (foundAsset != null)
                {
                    Logger.LogWarning($"Multiple assets found with name {name}, using first one.");
                    return foundAsset;
                }

                foundAsset = assetAtPath;
            }

            if (foundAsset == null)
            {
                Logger.LogWarning($"No asset with type {typeof(T).Name} found for the name {name}.");
                return false;
            }

            asset = foundAsset;
            return true;
        }

        public override Shader GetShader(string shaderOrMaterialName)
        {
            if (Shader.Find(shaderOrMaterialName) is { } shader)
            {
                return shader;
            }

            return base.GetShader(shaderOrMaterialName);
        }
    }
}