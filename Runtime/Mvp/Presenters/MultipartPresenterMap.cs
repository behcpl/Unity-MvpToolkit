using System.Collections.Generic;
using Behc.Mvp.Presenters.Factories;

namespace Behc.Mvp.Presenters
{
    public class MultipartPresenterMap : IPresenterMap
    {
        private readonly List<IPresenterMap> _maps;

        public MultipartPresenterMap()
        {
            _maps = new List<IPresenterMap>(4);
        }

        public void AddMap(IPresenterMap map)
        {
            // latest map goes first, it needs to be able to override other maps
            _maps.Insert(0, map);
        }

        public void RemoveMap(IPresenterMap map)
        {
            _maps.Remove(map);
        }

        public IPresenterFactory TryGetPresenterFactory(object model)
        {
            foreach (IPresenterMap map in _maps)
            {
                IPresenterFactory factory = map.TryGetPresenterFactory(model);
                if (factory != null)
                {
                    return factory;
                }
            }

            return null;
        }
    }
}