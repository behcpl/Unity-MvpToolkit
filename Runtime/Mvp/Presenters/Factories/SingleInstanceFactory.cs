﻿using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Behc.Mvp.Presenters.Factories
{
    public class SingleInstanceFactory : IPresenterFactory
    {
        private GameObject _instance;
        private readonly Transform _container;

        private IPresenter _presenter;
        private int _refCount;

        public SingleInstanceFactory(GameObject instance, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container)
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
        public static IDisposable RegisterInstance<T>(this PresenterMap presenterMap, GameObject instance, PresenterUpdateKernel updateKernel, RectTransform container)
        {
            return presenterMap.Register<T>(new SingleInstanceFactory(instance, presenterMap, updateKernel, container));
        }

        [MustUseReturnValue]
        public static IDisposable RegisterInstance<T>(this PresenterMap presenterMap, GameObject instance, PresenterUpdateKernel updateKernel, RectTransform container, Func<T, bool> predicate)
        {
            return presenterMap.Register<T>(new SingleInstanceFactory(instance, presenterMap, updateKernel, container), predicate);
        }
    }
}