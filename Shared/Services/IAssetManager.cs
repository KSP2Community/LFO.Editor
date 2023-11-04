using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace LFO.Shared
{
    public interface IAssetManager
    {
        public T GetAsset<T>(string name) where T : UnityObject;
        public Mesh GetMesh(string meshPath);
    }
}