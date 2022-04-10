using System;
using Behc.Utils;

namespace Behc.Configuration
{
    public class ConfiguratorSet : ITickable
    {
        public ConfiguratorStatus Status { get; private set; }

        private readonly IConfiguratorLoader[] _configuratorLoaders;
        private readonly IDependencyResolver _resolver;
        private readonly TickerManager _tickerManager;

        private Action _onComplete;

        public ConfiguratorSet(IConfiguratorLoader[] configuratorLoaders, IDependencyResolver resolver, TickerManager tickerManager)
        {
            _configuratorLoaders = configuratorLoaders;
            _resolver = resolver;
            _tickerManager = tickerManager;

            Status = ConfiguratorStatus.UNLOADED;
        }

        public void Load(Action onComplete)
        {
            if (Status != ConfiguratorStatus.UNLOADED)
                throw new Exception($"Invalid status: {Status} expected: {ConfiguratorStatus.UNLOADED}");

            Status = ConfiguratorStatus.LOADING;
            _onComplete = onComplete;
            _tickerManager.Track(this);
        }

        public void Unload(Action onComplete)
        {
            if (Status != ConfiguratorStatus.LOADED)
                throw new Exception($"Invalid status: {Status} expected: {ConfiguratorStatus.LOADED}");

            Status = ConfiguratorStatus.UNLOADING;
            _onComplete = onComplete;
            _tickerManager.Track(this);
        }

        public float GetLoadingProgress()
        {
            //TODO: add weights?

            float sum = 0;
            foreach (IConfiguratorLoader loader in _configuratorLoaders)
            {
                switch (loader.Status)
                {
                    case ConfiguratorStatus.LOADED:
                        sum += 1.0f;
                        break;
                    case ConfiguratorStatus.LOADING:
                        sum += loader.Progress;
                        break;
                }
            }

            return sum / _configuratorLoaders.Length;
        }

        public float GetUnloadingProgress()
        {
            //TODO: add weights?

            float sum = 0;
            foreach (IConfiguratorLoader loader in _configuratorLoaders)
            {
                switch (loader.Status)
                {
                    case ConfiguratorStatus.UNLOADED:
                        sum += 1.0f;
                        break;
                    case ConfiguratorStatus.UNLOADING:
                        sum += loader.Progress;
                        break;
                }
            }

            return sum / _configuratorLoaders.Length;
        }

        void ITickable.OnTick()
        {
            switch (Status)
            {
                case ConfiguratorStatus.LOADING:
                    ProcessLoading();
                    break;
                case ConfiguratorStatus.UNLOADING:
                    ProcessUnloading();
                    break;
            }
        }

        private void ProcessLoading()
        {
            foreach (IConfiguratorLoader loader in _configuratorLoaders)
            {
                if (loader.Status == ConfiguratorStatus.UNLOADED)
                    loader.Load(_resolver, null);

                if (loader.Status == ConfiguratorStatus.LOADED)
                    continue;

                return;
            }

            Action onComplete = _onComplete;
            _onComplete = null;
            Status = ConfiguratorStatus.LOADED;
            _tickerManager.Untrack(this);
            onComplete?.Invoke();
        }

        private void ProcessUnloading()
        {
            foreach (IConfiguratorLoader loader in _configuratorLoaders)
            {
                if (loader.Status == ConfiguratorStatus.LOADED)
                    loader.Unload(_resolver, null);

                if (loader.Status == ConfiguratorStatus.UNLOADED)
                    continue;

                return;
            }

            Action onComplete = _onComplete;
            _onComplete = null;
            Status = ConfiguratorStatus.UNLOADED;
            _tickerManager.Untrack(this);
            onComplete?.Invoke();
        }
    }

    public class ConfiguratorSetFactory : IFactory<IConfiguratorLoader[], ConfiguratorSet>
    {
        private readonly IDependencyResolver _dependencyResolver;
        private readonly TickerManager _tickerManager;

        public ConfiguratorSetFactory(IDependencyResolver dependencyResolver, TickerManager tickerManager)
        {
            _dependencyResolver = dependencyResolver;
            _tickerManager = tickerManager;
        }

        public ConfiguratorSet Create(IConfiguratorLoader[] loaders)
        {
            return new ConfiguratorSet(loaders, _dependencyResolver, _tickerManager);
        }
    }
}