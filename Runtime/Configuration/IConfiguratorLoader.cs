using System;
using Behc.MiniDi;

namespace Behc.Configuration
{
    public enum ConfiguratorStatus
    {
        UNLOADED,
        LOADED,
        UNLOADING,
        LOADING,
        DOWNLOADING,
        //TODO: error status
    }
    
    public interface IConfiguratorLoader
    {
        ConfiguratorStatus Status { get; }
        float Progress { get; }
        
        void Load(IDependencyResolver resolver, Action onComplete);
        void Unload(IDependencyResolver resolver, Action onComplete);
    }
}