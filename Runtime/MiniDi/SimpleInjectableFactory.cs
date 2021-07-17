using Behc.Mvp.Presenter;
using UnityEngine;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Behc.MiniDi
{
    public abstract class SimpleInjectableFactory : IPresenterFactory
    {
        private readonly GameObject _prefab;
        private readonly PresenterMap _presenterMap;
        private readonly PresenterUpdateKernel _updateKernel;

        protected SimpleInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel)
        {
            _prefab = prefab;
            _presenterMap = presenterMap;
            _updateKernel = updateKernel;
        }

        public IPresenter CreatePresenter(RectTransform parentTransform)
        {
            GameObject instance = Object.Instantiate(_prefab, parentTransform, false);
            IPresenter presenter = instance.GetComponent<IPresenter>();

            Inject(presenter);
            presenter.Initialize(_presenterMap, _updateKernel);
            instance.SetActive(false);

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

        protected abstract void Inject(IPresenter presenter);
    }

    public class SimpleInjectableFactory<T1> : SimpleInjectableFactory
    {
        private readonly T1 _param1;

        public SimpleInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, in T1 p1) : base(prefab, presenterMap, updateKernel)
        {
            _param1 = p1;
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1> injectable = (IInjectable<T1>) presenter;
            injectable.Inject(_param1);
        }
    }

    public class SimpleInjectableFactory<T1, T2> : SimpleInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;

        public SimpleInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, in T1 p1, in T2 p2)
            : base(prefab, presenterMap, updateKernel)
        {
            _param1 = p1;
            _param2 = p2;
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2> injectable = (IInjectable<T1, T2>) presenter;
            injectable.Inject(_param1, _param2);
        }
    }

    public class SimpleInjectableFactory<T1, T2, T3>
        : SimpleInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;

        public SimpleInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, in T1 p1, in T2 p2, in T3 p3)
            : base(prefab, presenterMap, updateKernel)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3> injectable = (IInjectable<T1, T2, T3>) presenter;
            injectable.Inject(_param1, _param2, _param3);
        }
    }

    public class SimpleInjectableFactory<T1, T2, T3, T4> : SimpleInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;

        public SimpleInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, in T1 p1, in T2 p2, in T3 p3, in T4 p4)
            : base(prefab, presenterMap, updateKernel)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4> injectable = (IInjectable<T1, T2, T3, T4>) presenter;
            injectable.Inject(_param1, _param2, _param3, _param4);
        }
    }
}