using LFO.Shared;
using LFO.Shared.Components;
using UnityEngine;
using UnityEditor;

namespace LFO.Editor.Utility
{
    internal static class LFOContextMenu
    {
        private static IAssetManager AssetManager => ServiceProvider.GetService<IAssetManager>();

        [MenuItem("GameObject/LFO/New Mesh Plume")]
        private static void CreateMeshPlume(MenuCommand command)
        {
            var parent = (GameObject)command.context;
            var go = new GameObject();
            go.AddComponent<SkinnedMeshRenderer>().sharedMesh = AssetManager.GetMesh("Flames");
            Object.DestroyImmediate(go.GetComponent<Collider>());

            go.AddComponent<LFOThrottleData>();
            go.GetComponent<Renderer>().sharedMaterial = new Material(AssetManager.GetShader("LFO/Additive"));

            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
                go.transform.localPosition = Vector3.zero;
            }

            go.transform.rotation *= Quaternion.Euler(-90, 0, 0);
        }

        [MenuItem("GameObject/LFO/New Volumetric Plume")]
        private static void CreateVolumetricPlume(MenuCommand command)
        {
            var parent = (GameObject)command.context;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Plume";
            Object.DestroyImmediate(go.GetComponent<Collider>());

            go.AddComponent<LFOVolume>();
            go.AddComponent<LFOThrottleData>();
            go.GetComponent<Renderer>().sharedMaterial = new Material(
                AssetManager.GetShader("LFO/Volumetric (Additive)")
            );

            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }

            go.transform.localScale = Vector3.one * 5;
        }

        [MenuItem("GameObject/LFO/New Volumetric Profiled Plume")]
        private static void CreateVolumetricProfiledPlume(MenuCommand command)
        {
            var parent = (GameObject)command.context;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Plume";
            Object.DestroyImmediate(go.GetComponent<Collider>());

            go.AddComponent<LFOVolume>();
            go.AddComponent<LFOThrottleData>();
            go.GetComponent<Renderer>().sharedMaterial = new Material(
                AssetManager.GetShader("LFO/Volumetric (Profiled)")
            );

            if (parent != null)
            {
                go.transform.SetParent(parent.transform);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }

            go.transform.localScale = Vector3.one * 5;
        }
    }
}