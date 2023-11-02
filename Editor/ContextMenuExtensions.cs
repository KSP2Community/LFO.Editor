using LFO.Shared.Components;
using UnityEngine;
using UnityEditor;

namespace LFO.Editor
{
    internal static class ContextMenuExtensions
    {
        [MenuItem("GameObject/LFO/New Volumetric Plume")]
        private static void CreateVolumetricPlume(MenuCommand command)
        {
            var parent = (GameObject)command.context;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Plume";
            Object.DestroyImmediate(go.GetComponent<Collider>());

            go.AddComponent<LFOVolume>();
            go.AddComponent<LFOThrottleData>();
            go.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("LFO/Volumetric (Additive)"));

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
            go.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("LFO/Volumetric (Profiled)"));

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