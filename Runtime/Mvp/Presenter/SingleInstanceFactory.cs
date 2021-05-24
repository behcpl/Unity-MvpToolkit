using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Behc.Mvp.Presenter
{
    public class SingleInstanceFactory : IPresenterFactory
    {
        private GameObject _instance;
        private readonly Transform _container;

        private IPresenter _presenter;
        private int _refCount;

        public SingleInstanceFactory(GameObject instance, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel)
        {
            _instance = instance;
            _container = container;

            _presenter = _instance.GetComponent<IPresenter>();
            _presenter.RectTransform.SetParent(_container, false);

            _presenter.Initialize(presenterMap, updateKernel);
            _instance.SetActive(false);
        }

        public IPresenter CreatePresenter(RectTransform parentTransform)
        {
            _refCount++;

            if (_presenter.RectTransform.parent != parentTransform)
                _presenter.RectTransform.SetParent(parentTransform, false);

            return _presenter;
        }

        public void DestroyPresenter(IPresenter presenter)
        {
            _refCount--;

            if (_refCount == 0)
            {
                if (_presenter.RectTransform.parent != _container)
                    _presenter.RectTransform.SetParent(_container, false);

                _instance.SetActive(false);
            }
        }

        public void Dispose()
        {
            Object.Destroy(_instance);
            _instance = null;
            _presenter = null;
        }
    }

    public static class SingleInstanceFactoryExtensions
    {
        [MustUseReturnValue]
        public static IDisposable RegisterInstance<T>(this PresenterMap presenterMap, GameObject instance, RectTransform container, PresenterUpdateKernel updateKernel)
        {
            return presenterMap.Register<T>(new SingleInstanceFactory(instance, container, presenterMap, updateKernel));
        }

        [MustUseReturnValue]
        public static IDisposable RegisterInstance<T>(this PresenterMap presenterMap, GameObject instance, RectTransform container, PresenterUpdateKernel updateKernel, Func<T, bool> predicate)
        {
            return presenterMap.Register<T>(new SingleInstanceFactory(instance, container, presenterMap, updateKernel), predicate);
        }
    }
}