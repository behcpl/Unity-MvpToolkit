#if BEHC_MVPTOOLKIT_ADDRESSABLES
using System;
using Behc.MiniDi;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Behc.Configuration.Loaders
{
    public class AddressableSceneConfiguratorLoader : IConfiguratorLoader
    {
        public ConfiguratorStatus Status { get; private set; }

        public float Progress => _handle.IsValid() ? _handle.PercentComplete : 1.0f;

        private readonly object _key;
        private AsyncOperationHandle<SceneInstance> _handle;
        private readonly bool _removeProxyObjects;

        public AddressableSceneConfiguratorLoader(object key, bool removeProxyObjects = true)
        {
            _key = key;
            _removeProxyObjects = removeProxyObjects;
            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.UNLOADED, $"Invalid status: '{Status}' for '{_key}' scene!");

            void Finished(AsyncOperationHandle<SceneInstance> obj)
            {
                Status = ConfiguratorStatus.LOADED;

                bool rootElementFound = false;
                Scene scene = _handle.Result.Scene;
                foreach (GameObject rootObj in scene.GetRootGameObjects())
                {
                    IConfigurator root = rootObj.GetComponent<IConfigurator>();
                    if (root == null)
                        continue;

                    rootElementFound = true;
                    try
                    {
                        root.Load(resolver);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                if (!rootElementFound)
                    Debug.LogError($"Missing scene configurator object for: '{scene.name}'");

                //cleanup in 2nd pass - some root object could be reparented during root.Load()
                if (_removeProxyObjects)
                {
                    foreach (GameObject rootObj in scene.GetRootGameObjects())
                    {
                        IConfigurator root = rootObj.GetComponent<IConfigurator>();
                        if (root == null)
                        {
                            UnityEngine.Object.Destroy(rootObj);
                        }
                    }
                }

                onComplete?.Invoke();
            }

            Status = ConfiguratorStatus.LOADING;
            _handle = Addressables.LoadSceneAsync(_key, LoadSceneMode.Additive);
            _handle.Completed += Finished;
        }

        public void Unload(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.LOADED, $"Invalid status: '{Status}' for '{_key}' scene!");

            Scene scene = _handle.Result.Scene;
            foreach (GameObject rootObj in scene.GetRootGameObjects())
            {
                try
                {
                    IConfigurator root = rootObj.GetComponent<IConfigurator>();
                    root?.Unload(resolver);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            Status = ConfiguratorStatus.UNLOADING;

            Addressables.UnloadSceneAsync(_handle.Result).Completed += h =>
            {
                Status = ConfiguratorStatus.UNLOADED;
                _handle = default;
                onComplete?.Invoke();
            };
        }
    }
}
#endif