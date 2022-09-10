using System.Collections.Generic;
using Behc.Mvp.Presenter;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Behc.Mvp.Presenters.Factories
{
    public abstract class PooledInjectableFactory : IPresenterFactory
    {
        private readonly GameObject _prefab;
        private readonly PresenterMap _presenterMap;
        private readonly PresenterUpdateKernel _updateKernel;
        private readonly Transform _container;
        private readonly List<IPresenter> _unused = new List<IPresenter>();
        private readonly int _maximumPoolSize;

        private PresenterMap _localPresenterMap;

        protected PooledInjectableFactory(GameObject prefab, Transform container, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, int maximumPoolSize = int.MaxValue)
        {
            _prefab = prefab;
            _container = container;
            _presenterMap = presenterMap;
            _updateKernel = updateKernel;
            _maximumPoolSize = maximumPoolSize;
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

        public PresenterMap LocalPresenterMap()
        {
            _localPresenterMap ??= new PresenterMap(_presenterMap);
            return _localPresenterMap;
        }

        protected abstract void Inject(IPresenter presenter);

        protected void InitializePool(int initialPoolSize)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                _unused.Add(CreateNewObject(_container));
            }
        }

        private IPresenter CreateNewObject(Transform container)
        {
            GameObject instance = Object.Instantiate(_prefab, container, false);
            IPresenter presenter = instance.GetComponent<IPresenter>();
            presenter.Initialize(_localPresenterMap ?? _presenterMap, _updateKernel);
            Inject(presenter);
            instance.SetActive(false);
            return presenter;
        }
    }

    public class PooledInjectableFactory<T1> : PooledInjectableFactory
    {
        private readonly T1 _param1;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1> injectable = (IInjectable<T1>)presenter;
            injectable.Inject(_param1);
        }
    }

    public class PooledInjectableFactory<T1, T2> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2> injectable = (IInjectable<T1, T2>)presenter;
            injectable.Inject(_param1, _param2);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3> injectable = (IInjectable<T1, T2, T3>)presenter;
            injectable.Inject(_param1, _param2, _param3);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3, T4> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, in T4 p4, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4> injectable = (IInjectable<T1, T2, T3, T4>)presenter;
            injectable.Inject(_param1, _param2, _param3, _param4);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3, T4, T5> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;
        private readonly T5 _param5;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, in T4 p4, in T5 p5, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
            _param5 = p5;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4, T5> injectable = (IInjectable<T1, T2, T3, T4, T5>)presenter;
            injectable.Inject(_param1, _param2, _param3, _param4, _param5);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3, T4, T5, T6> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;
        private readonly T5 _param5;
        private readonly T6 _param6;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, in T4 p4, in T5 p5, in T6 p6, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
            _param5 = p5;
            _param6 = p6;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4, T5, T6> injectable = (IInjectable<T1, T2, T3, T4, T5, T6>)presenter;
            injectable.Inject(_param1, _param2, _param3, _param4, _param5, _param6);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3, T4, T5, T6, T7> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;
        private readonly T5 _param5;
        private readonly T6 _param6;
        private readonly T7 _param7;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, in T4 p4, in T5 p5, in T6 p6, in T7 p7, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
            _param5 = p5;
            _param6 = p6;
            _param7 = p7;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4, T5, T6, T7> injectable = (IInjectable<T1, T2, T3, T4, T5, T6, T7>)presenter;
            injectable.Inject(_param1, _param2, _param3, _param4, _param5, _param6, _param7);
        }
    }

    public class PooledInjectableFactory<T1, T2, T3, T4, T5, T6, T7, T8> : PooledInjectableFactory
    {
        private readonly T1 _param1;
        private readonly T2 _param2;
        private readonly T3 _param3;
        private readonly T4 _param4;
        private readonly T5 _param5;
        private readonly T6 _param6;
        private readonly T7 _param7;
        private readonly T8 _param8;

        public PooledInjectableFactory(GameObject prefab, PresenterMap presenterMap, PresenterUpdateKernel updateKernel, Transform container, in T1 p1, in T2 p2, in T3 p3, in T4 p4, in T5 p5, in T6 p6, in T7 p7, in T8 p8, int initialPoolSize = 0, int maximumPoolSize = int.MaxValue)
            : base(prefab, container, presenterMap, updateKernel, maximumPoolSize)
        {
            _param1 = p1;
            _param2 = p2;
            _param3 = p3;
            _param4 = p4;
            _param5 = p5;
            _param6 = p6;
            _param7 = p7;
            _param8 = p8;
            InitializePool(initialPoolSize);
        }

        protected override void Inject(IPresenter presenter)
        {
            IInjectable<T1, T2, T3, T4, T5, T6, T7, T8> injectable = (IInjectable<T1, T2, T3, T4, T5, T6, T7, T8>)presenter;
            injectable.Inject(_param1, _param2, _param3, _param4, _param5, _param6, _param7, _param8);
        }
    }
}