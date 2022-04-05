using System;
using Behc.MiniDi;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Behc.Configuration.Loaders
{
    public class BuildSceneConfiguratorLoader : IConfiguratorLoader
    {
        public ConfiguratorStatus Status { get; private set; }

        public float Progress => _asyncOperation?.progress ?? 1.0f;

        private readonly string _sceneName;
        private readonly bool _removeProxyObjects;
        private readonly LocalPhysicsMode _physicsMode;

        private AsyncOperation _asyncOperation;

        public BuildSceneConfiguratorLoader(string sceneName, bool removeProxyObjects = true, LocalPhysicsMode physicsMode = LocalPhysicsMode.None)
        {
            _sceneName = sceneName;
            _removeProxyObjects = removeProxyObjects;
            _physicsMode = physicsMode;
            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.UNLOADED, $"Invalid status: '{Status}' for '{_sceneName}' scene!");

            void Finished()
            {
                _asyncOperation = null;
                Status = ConfiguratorStatus.LOADED;

                bool rootElementFound = false;
                Scene scene = SceneManager.GetSceneByName(_sceneName);
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
                    Debug.LogError($"Missing scene configurator object for: '{_sceneName}'");

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

            // No need to check result.isDone, += will do that and call Finished() sync
            Status = ConfiguratorStatus.LOADING;
            _asyncOperation = SceneManager.LoadSceneAsync(_sceneName, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive, localPhysicsMode = _physicsMode });
            _asyncOperation.completed += op => Finished();
        }

        public void Unload(IDependencyResolver resolver, Action onComplete)
        {
            Debug.Assert(Status == ConfiguratorStatus.LOADED, $"Invalid status: '{Status}' for '{_sceneName}' scene!");

            Scene scene = SceneManager.GetSceneByName(_sceneName);

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
            _asyncOperation = SceneManager.UnloadSceneAsync(scene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            _asyncOperation.completed += op =>
            {
                Status = ConfiguratorStatus.UNLOADED;
                _asyncOperation = null;
                onComplete?.Invoke();
            };
        }
    }
}