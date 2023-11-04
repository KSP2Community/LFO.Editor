using UnityEngine;

namespace LFO.Shared.Components
{
    [ExecuteInEditMode, RequireComponent(typeof(Renderer))]
    public class LFOVolume : MonoBehaviour
    {
        private static Resolution _resolution;
        public Resolution Resolution = Resolution.Medium;

        private Material Material => Application.isEditor
            ? GetComponent<Renderer>().sharedMaterial
            : GetComponent<Renderer>().material;

        private void Start()
        {
            if (Material == null)
            {
                return;
            }

            Material.SetFloat("_TimeOffset", Random.Range(-10f, 10f));
            Material.SetInt("_Resolution", (int)_resolution);
            Material.SetVector("scale", transform.lossyScale);
            Material.SetMatrix("rotation", Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation)));
            Material.SetVector("position", transform.position);
        }

        private void LateUpdate()
        {
            if (Material == null)
            {
                return;
            }

            Material.SetVector("scale", transform.lossyScale);
            Material.SetMatrix("rotation", Matrix4x4.Rotate(Quaternion.Inverse(transform.rotation)));
            Material.SetVector("position", transform.position);

            if (Resolution == _resolution)
            {
                return;
            }

            _resolution = Resolution;
            Material.SetInt("_Resolution", (int)_resolution);
        }
    }

    public enum Resolution
    {
        Minimal,
        Low,
        Medium,
        High,
        Extreme
    }
}