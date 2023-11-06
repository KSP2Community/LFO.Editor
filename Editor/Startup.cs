using LFO.Editor.Services;
using LFO.Shared;
using UnityEditor;
using ILogger = LFO.Shared.ILogger;

namespace LFO.Editor
{
    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            ServiceProvider.RegisterService<ILogger>(new UnityLogger());
            ServiceProvider.RegisterService<IAssetManager>(new UnityAssetManager());
        }
    }
}