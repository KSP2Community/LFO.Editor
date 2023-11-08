using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace LFO.Shared
{
    public abstract class BaseAssetManager : IAssetManager
    {
        private static readonly Dictionary<string, string> RenamedAssets = new()
        {
            { "vfx_exh_bell_j_01_0", "bell_j_1" },
            { "vfx_exh_bell_p2_1_0", "bell_p2_1" },
            { "vfx_exh_shock_p1_s1_0", "shock_1_pt1" },
            { "vfx_exh_shock_p2_s1_0", "shock_1_pt2" },
            { "vfx_exh_shock_p3_s1_0", "shock_1_pt3" },
            { "vfx_exh_shock_p4_s1_0", "shock_1_pt4" },
            { "LFOAdditive 2.0", "LFO/Additive" },
        };

        public abstract string GetAssetPath<T>(string name);

        public abstract T GetAsset<T>(string name) where T : UnityObject;

        public abstract bool TryGetAsset<T>(string name, out T asset) where T : UnityObject;

        public virtual Mesh GetMesh(string meshName)
        {
            if (GetAsset<GameObject>(meshName) is { } fbxPrefab)
            {
                return fbxPrefab.TryGetComponent(out SkinnedMeshRenderer skinnedRenderer)
                    ? skinnedRenderer.sharedMesh
                    : fbxPrefab.GetComponent<MeshFilter>().sharedMesh;
            }

            // obj's meshes are named "meshName_#" with # being the meshID
            return GetAsset<GameObject>(meshName.Remove(meshName.Length - 2))
                ?.GetComponentInChildren<MeshFilter>()
                ?.sharedMesh;
        }

        public virtual Shader GetShader(string shaderOrMaterialName)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            if (TryGetAsset(shaderOrMaterialName, out Shader shader))
            {
                return shader;
            }

            if (TryGetAsset(shaderOrMaterialName, out Material material))
            {
                return material.shader;
            }

            return null;
        }

        [CanBeNull]
        protected string GetRenamedAssetName(string name)
        {
            return RenamedAssets.TryGetValue(name.ToLowerInvariant(), out string newName)
                ? newName
                : null;
        }
    }
}

