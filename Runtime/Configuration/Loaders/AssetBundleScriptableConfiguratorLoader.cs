using System;
using UnityEngine;

namespace Behc.Configuration.Loaders
{
    //simple asset bundle loader, does not track any usage of a given bundle
    public class AssetBundleScriptableConfiguratorLoader : IConfiguratorLoader
    {
        public ConfiguratorStatus Status { get; private set; }

        public float Progress => _asyncOperation?.progress ?? 1.0f;

        private readonly AssetBundle _bundle;
        private readonly string _assetName;
        private readonly bool _canUnloadBundle;

        private AssetBundleRequest _asyncOperation;
        private IConfigurator _configurator;

        public AssetBundleScriptableConfiguratorLoader(AssetBundle bundle, string assetName, bool canUnloadBundle)
        {
            _bundle = bundle;
            _assetName = assetName;
            _canUnloadBundle = canUnloadBundle;

            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.UNLOADED, $"Invalid status: '{Status}' for '{_bundle.name}:{_assetName}' scriptable configurator!");

            void Finished(AsyncOperation _)
            {
                _configurator = (IConfigurator)_asyncOperation.asset;
                _asyncOperation = null;

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
            _asyncOperation = _bundle.LoadAssetAsync<ScriptableConfigurator>(_assetName);
            _asyncOperation.completed += Finished;
        }

        public void Unload(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.LOADED, $"Invalid status: '{Status}' for '{_bundle.name}:{_assetName}' scriptable configurator!");

            try
            {
                _configurator.Unload(resolver);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _configurator = null;
            if (_canUnloadBundle)
            {
                _bundle.Unload(true);
            }

            Status = ConfiguratorStatus.UNLOADED;
            onComplete?.Invoke();
        }
    }
}