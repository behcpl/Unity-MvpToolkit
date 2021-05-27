using System.Collections.Generic;
using Behc.Mvp.Presenter;
using UnityEngine;

namespace Behc.MiniDi
{
    public abstract class PooledInjectableFactory : IPresenterFactory
    {
        private readonly GameObject _prefab;
        private readonly PresenterMap _presenterMap;
        private readonly PresenterUpdateKernel _updateKernel;
        private readonly Transform _container;
        private readonly List<IPresenter> _unused = new List<IPresenter>();

        protected PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int initialPoolSize)
        {
            _prefab = prefab;
            _container = container;
            _presenterMap = presenterMap;
            _updateKernel = updateKernel;

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

        protected abstract void Inject(IPresenter presenter);

        private IPresenter CreateNewObject(Transform container)
        {
            GameObject instance = Object.Instantiate(_prefab, container, false);
            IPresenter presenter = instance.GetComponent<IPresenter>();
            presenter.Initialize(_presenterMap, _updateKernel);
            Inject(presenter);
            instance.SetActive(false);
            return presenter;
        }
    }

    public class PooledInjectableFactory<T1> : PooledInjectableFactory
    {
        private readonly T1 _param1;

        public PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int initialPoolSize, in T1 p1)
            : base(prefab, container, presenterMap, updateKernel, initialPoolSize)
        {
            _param1 = p1;
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1> injectable = (IInjectable<T1>) presenter;
            injectable.Inject(_param1);
        }
    }

    public class PooledInjectableFactory<T1, T2> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;

        public PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int initialPoolSize, in T1 p1, in T2 p2)
            : base(prefab, container, presenterMap, updateKernel, initialPoolSize)
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

    public class PooledInjectableFactory<T1, T2, T3> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;

        public PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int initialPoolSize, in T1 p1, in T2 p2, in T3 p3)
            : base(prefab, container, presenterMap, updateKernel, initialPoolSize)
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

    public class PooledInjectableFactory<T1, T2, T3, T4> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;

        public PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int initialPoolSize, in T1 p1, in T2 p2, in T3 p3, in T4 p4)
            : base(prefab, container, presenterMap, updateKernel, initialPoolSize)
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