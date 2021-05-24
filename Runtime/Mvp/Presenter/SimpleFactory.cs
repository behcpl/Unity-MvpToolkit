using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Behc.Mvp.Presenter
{
    public class SimpleFactory : IPresenterFactory
    {
        private readonly GameObject _prefab;
        private readonly PresenterMap _presenterMap;
        private readonly PresenterUpdateKernel _updateKernel;

        public SimpleFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel)
        {
            _prefab = prefab;
            _presenterMap = presenterMap;
            _updateKernel = updateKernel;
        }

        public IPresenter CreatePresenter(RectTransform parentTransform)
        {
            GameObject instance = Object.Instantiate(_prefab, parentTransform, false);
            IPresenter presenter = instance.GetComponent<IPresenter>();

            presenter.Initialize(_presenterMap, _updateKernel);

            return presenter;
        }

        public void DestroyPresenter(IPresenter presenter)
        {
            presenter.Destroy();

            Object.Destroy(presenter.RectTransform.gameObject);
        }

        public void Dispose()
        {
            //NOOP
        }
    }

    public static class SimpleFactoryExtensions
    {
        [MustUseReturnValue]
        public static IDisposable RegisterFactory<T>(this PresenterMap presenterMap, GameObject prefab, PresenterUpdateKernel updateKernel)
        {
            return presenterMap.Register<T>(new SimpleFactory(prefab, presenterMap, updateKernel));
        }

        [MustUseReturnValue]
        public static IDisposable RegisterFactory<T>(this PresenterMap presenterMap, GameObject prefab, PresenterUpdateKernel updateKernel, Func<T, bool> predicate)
        {
            return presenterMap.Register<T>(new SimpleFactory(prefab, presenterMap, updateKernel), predicate);
        }
    }
}