using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace LFO.Shared
{
    public interface IAssetManager
    {
        public T GetAsset<T>(string name) where T : UnityObject;
        public bool TryGetAsset<T>(string name, out T asset) where T : UnityObject;
        public Mesh GetMesh(string meshName);
        public Shader GetShader(string shaderOrMaterialName);
    }
}