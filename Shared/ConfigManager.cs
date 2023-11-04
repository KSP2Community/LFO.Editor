using System.Collections.Generic;
using LFO.Shared.Configs;
using UnityEngine;

namespace LFO.Shared
{
    public class ConfigManager
    {
        public static ConfigManager Instance => _instance ??= new ConfigManager();

        private static ConfigManager _instance;

        public readonly Dictionary<string, LFOConfig> PartNameToConfigDict = new();

        public readonly Dictionary<string, Dictionary<string, PlumeConfig>> GameObjectToPlumeDict = new();

        public static void RegisterLFOConfig(string partName, LFOConfig config)
        {
            Instance.PartNameToConfigDict.TryAdd(partName, new LFOConfig());

            if (!Instance.GameObjectToPlumeDict.ContainsKey(partName))
            {
                Instance.GameObjectToPlumeDict.Add(partName, new Dictionary<string, PlumeConfig>());
            }

            Instance.PartNameToConfigDict[partName] = config;
        }

        public static LFOConfig GetConfig(string partName)
        {
            return Instance.PartNameToConfigDict.ContainsKey(partName)
                ? Instance.PartNameToConfigDict[partName]
                : null;
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
                Debug.LogWarning($"[LFO] {partName} has no registered plume");
            }
        }

        internal static bool TryGetPlumeConfig(string partName, string id, out PlumeConfig config)
        {
            if (Instance.GameObjectToPlumeDict.ContainsKey(partName))
            {
                return Instance.GameObjectToPlumeDict[partName].TryGetValue(id, out config);
            }

            Debug.LogWarning($"[LFO] {partName} has no registered plume");
            config = null;
            return false;
        }
    }
}