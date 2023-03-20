using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Behc.Mvp.Presenters.Factories
{
    public class PooledFactory : IPresenterFactory
    {
        private readonly GameObject _prefab;
        private readonly PresenterMap _presenterMap;
        private readonly PresenterUpdateKernel _updateKernel;
        private readonly Transform _container;
        private readonly List<IPresenter> _unused = new List<IPresenter>();
        private readonly int _maximumPoolSize;

        public PooledFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
        {
            _prefab = prefab;
            _presenterMap = presenterMap;
            _updateKernel = updateKernel;
            _container = container;
            _maximumPoolSize = maximumPoolSize;

            //TODO: this doesn't work with LocalPresenterMap
            for (int i = 0; i < initialPoolSize; i++)
                _unused.Add(CreateNewObject(_container));
        }

        public IPresenter CreatePresenter(RectTransform parentTransform)
        {
            if (_unused.Count > 0)
            {
                IPresenter presenter = _unused[_unused.Count - 1];
                _unused.RemoveAt(_unused.Count - 1);

                if (presenter.RectTransform.parent != parentTransform)
                    presenter.RectTransform.SetParent(parentTransform, false);

                return presenter;
            }

            return CreateNewObject(parentTransform);
        }

        public void DestroyPresenter(IPresenter presenter)
        {
            if (_unused.Count >= _maximumPoolSize)
            {
                presenter.Destroy();
                Object.Destroy(presenter.RectTransform.gameObject);
                return;
            }

            if (presenter.RectTransform.parent != _container)
                presenter.RectTransform.SetParent(_container, false);

            _unused.Add(presenter);
        }

        public void Dispose()
        {
            foreach (IPresenter presenter in _unused)
            {
                presenter.Destroy();
                Object.Destroy(presenter.RectTransform.gameObject);
            }
        }

        private IPresenter CreateNewObject(Transform container)
        {
            GameObject instance = Object.Instantiate(_prefab, container, false);
            IPresenter presenter = instance.GetComponent<IPresenter>();
            presenter.Initialize(_presenterMap, _updateKernel);
            instance.SetActive(false);

            return presenter;
        }
    }

    public static class PooledFactoryExtensions
    {
        [MustUseReturnValue]
        public static IDisposable RegisterPool<T>(this PresenterMap presenterMap, GameObject prefab, PresenterUpdateKernel updateKernel, RectTransform container, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
        {
            return presenterMap.Register<T>(new PooledFactory(prefab, presenterMap, updateKernel, container, initialPoolSize, maximumPoolSize));
        }

        [MustUseReturnValue]
        public static IDisposable RegisterPool<T>(this PresenterMap presenterMap, GameObject prefab, PresenterUpdateKernel updateKernel, RectTransform container, Func<T, bool> predicate, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
        {
            return presenterMap.Register<T>(new PooledFactory(prefab, presenterMap, updateKernel, container, initialPoolSize, maximumPoolSize), predicate);
        }
    }
}