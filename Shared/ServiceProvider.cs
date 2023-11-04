using System;
using System.Collections.Generic;

namespace LFO.Shared
{
    public static class ServiceProvider
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void RegisterService<T>(T service) where T : class
        {
            Services.Add(typeof(T), service);
        }

        public static T GetService<T>()
        {
            return Services.TryGetValue(typeof(T), out var service) ? (T)service : default;
        }
    }
}