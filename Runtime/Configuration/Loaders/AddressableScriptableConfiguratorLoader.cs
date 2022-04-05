#if BEHC_MVPTOOLKIT_ADDRESSABLES
using System;
using Behc.MiniDi;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Behc.Configuration.Loaders
{
    public class AddressableScriptableConfiguratorLoader : IConfiguratorLoader
    {
        public ConfiguratorStatus Status { get; private set; }

        public float Progress => _handle.IsValid() ? _handle.PercentComplete : 1.0f;

        private readonly object _key;
        private AsyncOperationHandle<ScriptableConfigurator> _handle;

        public AddressableScriptableConfiguratorLoader(object key)
        {
            _key = key;
            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.UNLOADED, $"Invalid status: '{Status}' for '{_key}' scriptable configurator!");

            void Finished(AsyncOperationHandle<ScriptableConfigurator> obj)
            {
                IConfigurator configurator = obj.Result;
                try
                {
                    configurator.Load(resolver);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Status = ConfiguratorStatus.LOADED;
                onComplete?.Invoke();
            }

            Status = ConfiguratorStatus.LOADING;
            _handle = Addressables.LoadAssetAsync<ScriptableConfigurator>(_key);
            _handle.Completed += Finished;
        }

        public void Unload(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.LOADED, $"Invalid status: '{Status}' for '{_key}' scriptable configurator!");

            IConfigurator configurator = _handle.Result;
            configurator.Unload(resolver);

            Addressables.Release(_handle);
            _handle = default;

            Status = ConfiguratorStatus.UNLOADED;
            onComplete?.Invoke();
        }
    }
}
#endif