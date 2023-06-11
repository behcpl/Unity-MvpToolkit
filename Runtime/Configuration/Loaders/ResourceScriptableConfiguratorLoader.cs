using System;
using UnityEngine;

namespace Behc.Configuration.Loaders
{
    public class ResourceScriptableConfiguratorLoader : IConfiguratorLoader
    {
        public ConfiguratorStatus Status { get; private set; }
        public float Progress => _resourceRequest?.progress ?? 1.0f;

        private readonly string _resourcePath;

        private ResourceRequest _resourceRequest;
        private IConfigurator _configurator;

        public ResourceScriptableConfiguratorLoader(string resourcePath)
        {
            _resourcePath = resourcePath;

            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.UNLOADED, $"Invalid status: '{Status}' for '{_resourcePath}' scriptable configurator!");

            void Finished(AsyncOperation _)
            {
                _configurator = (IConfigurator)_resourceRequest.asset;
                _resourceRequest = null;

                try
                {
                    _configurator.Load(resolver);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Status = ConfiguratorStatus.LOADED;
                onComplete?.Invoke();
            }

            Status = ConfiguratorStatus.LOADING;
            _resourceRequest = Resources.LoadAsync<ScriptableConfigurator>(_resourcePath);
            _resourceRequest.completed += Finished;
        }

        public void Unload(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.LOADED, $"Invalid status: '{Status}' for '{_resourcePath}' scriptable configurator!");

            try
            {
                _configurator.Unload(resolver);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _configurator = null;

            Status = ConfiguratorStatus.UNLOADED;
            onComplete?.Invoke();
        }
    }
}