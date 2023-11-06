using System;
using LFO.Shared;
using UnityEditor;

namespace LFO.Editor.Services
{
    public class UnityAssetManager : BaseAssetManager
    {
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

                throw new Exception($"No asset found with name {name}");
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
                    throw new Exception($"Multiple assets found with name {name}");
                }

                foundAsset = asset;
            }

            if (foundAsset == null)
            {
                throw new Exception($"No asset found with name {name} and type {typeof(T).Name}");
            }

            return foundAsset;
        }

        public override bool TryGetAsset<T>(string name, out T asset)
        {
            asset = null;

            string[] foundGuids = AssetDatabase.FindAssets(name, AssetFolders);
            if (foundGuids.Length == 0)
            {
                return GetRenamedAssetName(name) != null && TryGetAsset(GetRenamedAssetName(name), out asset);
            }

            T foundAsset = null;
            foreach (string guid in foundGuids)
            {
                var assetAtPath = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (assetAtPath == null)
                {
                    continue;
                }

                if (foundAsset != null)
                {
                    return false;
                }

                foundAsset = assetAtPath;
            }

            if (foundAsset == null)
            {
                return false;
            }

            asset = foundAsset;
            return true;

        }
    }
}