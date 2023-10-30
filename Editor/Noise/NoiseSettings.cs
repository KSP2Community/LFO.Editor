using UnityEngine;

namespace LFO.Editor.Noise
{
    public abstract class NoiseSettings : ScriptableObject
    {
        public event System.Action OnValueChanged;

        public abstract System.Array GetDataArray();

        public abstract int Stride { get; }

        private void OnValidate()
        {
            OnValueChanged?.Invoke();
        }
    }
}