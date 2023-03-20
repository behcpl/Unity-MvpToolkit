using System;
using System.Collections.Generic;
using Behc.Mvp.Presenters.Factories;
using Behc.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public class PresenterMap
    {
        private class MapItem
        {
            public IPresenterFactory DefaultFactory;
            public List<(IPresenterFactory factory, Func<object, bool> predicate)> SpecializedFactories;
        }

        private readonly PresenterMap _parentMap;
        private Dictionary<Type, MapItem> _map;

        public PresenterMap(PresenterMap parentMap)
        {
            _parentMap = parentMap;
        }

        [MustUseReturnValue]
        public IDisposable Register<T>(IPresenterFactory factory)
        {
            return AddDefaultFactory(typeof(T), factory);
        }

        [MustUseReturnValue]
        public IDisposable Register<T>(IPresenterFactory factory, Func<T, bool> predicate)
        {
            return AddPredicatedFactory(typeof(T), factory, obj => predicate((T)obj));
        }

        public IPresenter CreatePresenter(object model, RectTransform contentTransform)
        {
            IPresenterFactory factory = TryGetPresenterFactory(model);
            if(factory == null)
                throw new Exception($"No PresenterFactory found for '{model.GetType().Name}' model!");
            
            return factory.CreatePresenter(contentTransform);
        }

        public void DestroyPresenter(object model, IPresenter presenter)
        {
            IPresenterFactory factory = TryGetPresenterFactory(model);
            if(factory == null)
                throw new Exception($"No PresenterFactory found for '{model.GetType().Name}' model!");
 
            factory.DestroyPresenter(presenter);
        }

        public IPresenterFactory TryGetPresenterFactory(object model)
        {
            if (_map != null && _map.TryGetValue(model.GetType(), out MapItem item))
            {
                if (item.SpecializedFactories != null)
                {
                    foreach ((IPresenterFactory factory, Func<object,bool> predicate) in item.SpecializedFactories)
                    {
                        if (predicate(model))
                        {
                            return factory;
                        }
                    }
                }

                return item.DefaultFactory;
            }
            
            return _parentMap?.TryGetPresenterFactory(model);
        }

        private IDisposable AddDefaultFactory(Type modelType, IPresenterFactory presenterFactory)
        {
            MapItem mapItem = GetOrCreateMapItem(modelType);        

            if (mapItem.DefaultFactory != null)
                throw new Exception($"Default factory for model '{modelType.Name}' already exist!");

            mapItem.DefaultFactory = presenterFactory;
            return new GenericDisposable(() =>
            {
                presenterFactory.Dispose();
                mapItem.DefaultFactory = null;
            });
        }

        private IDisposable AddPredicatedFactory(Type modelType, IPresenterFactory presenterFactory, Func<object, bool> wrappedPredicate)
        {
            MapItem mapItem = GetOrCreateMapItem(modelType);
            mapItem.SpecializedFactories ??= new List<(IPresenterFactory factory, Func<object, bool> predicate)>();

            var tuple = (presenterFactory, wrappedPredicate);
            mapItem.SpecializedFactories.Add(tuple);
            
            return new GenericDisposable(() =>
            {
                presenterFactory.Dispose();
                mapItem.SpecializedFactories.Remove(tuple);
            });
        }

        private MapItem GetOrCreateMapItem(Type modelType)
        {
            _map ??= new Dictionary<Type, MapItem>();

            if (_map.TryGetValue(modelType, out MapItem mapItem))
                return mapItem;
            
            mapItem = new MapItem();
            _map.Add(modelType, mapItem);

            return mapItem;
        }
    }
}