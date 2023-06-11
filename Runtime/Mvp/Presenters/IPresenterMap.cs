using System;
using Behc.Mvp.Presenters.Factories;
using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public interface IPresenterMap
    {
        public IPresenterFactory TryGetPresenterFactory(object model);
    }

    public static class PresenterMapExtensions
    {
        public static IPresenter CreatePresenter(this IPresenterMap map, object model, RectTransform contentTransform)
        {
            IPresenterFactory factory = map.TryGetPresenterFactory(model);
            if (factory == null)
                throw new Exception($"No PresenterFactory found for '{model.GetType().Name}' model!");

            return factory.CreatePresenter(contentTransform);
        }

        public static void DestroyPresenter(this IPresenterMap map, object model, IPresenter presenter)
        {
            IPresenterFactory factory = map.TryGetPresenterFactory(model);
            if (factory == null)
                throw new Exception($"No PresenterFactory found for '{model.GetType().Name}' model!");

            factory.DestroyPresenter(presenter);
        }
    }
}