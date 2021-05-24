using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Mvp.Presenter
{
    public class PresenterMap
    {
        private readonly List<MapItem> _map = new List<MapItem>();
        private readonly PresenterMap _parentMap;

        public PresenterMap(PresenterMap parentMap)
        {
            _parentMap = parentMap;
        }

       [MustUseReturnValue]
        public IDisposable Register<T>(IPresenterFactory factory)
        {
            return AddMapItem(new MapItem
            {
                ModelType = typeof(T),
                Factory = factory
            });
        }

        [MustUseReturnValue]
        public IDisposable Register<T>(IPresenterFactory factory, Func<T, bool> predicate)
        {
            return AddMapItem(new MapItem
            {
                ModelType = typeof(T),
                Factory = factory,
                Predicate = obj => predicate((T) obj)
            });
        }

        public IPresenter CreatePresenter(object model, RectTransform contentTransform)
        {
            return GetDescriptor(model).Factory.CreatePresenter(contentTransform);
        }

        public void DestroyPresenter(object model, IPresenter presenter)
        {
            GetDescriptor(model).Factory.DestroyPresenter(presenter);
        }

        private MapItem GetDescriptor(object model)
        {
            MapItem found = _map.Find(item => item.ModelType == model.GetType() && (item.Predicate?.Invoke(model) ?? true));
            if (found != null)
                return found;

            if (_parentMap != null)
                return _parentMap.GetDescriptor(model);

            throw new Exception($"No Presenter found for '{model.GetType().Name}' model!");
        }

        private IDisposable AddMapItem(MapItem item)
        {
            _map.Add(item);
            return new MapItemUnregister(_map, item);
        }

        private class MapItem
        {
            public Type ModelType;
            public IPresenterFactory Factory;
            public Func<object, bool> Predicate;
        }

        private class MapItemUnregister : IDisposable
        {
            private readonly List<MapItem> _map;
            private readonly MapItem _item;

            public MapItemUnregister(List<MapItem> map, MapItem item)
            {
                _map = map;
                _item = item;
            }

            public void Dispose()
            {
                _item.Factory.Dispose();
                _map.Remove(_item);
            }
        }
    }
}