using System.Collections.Generic;
using System.Linq;
using Behc.Mvp.Models;
using UnityEngine;

namespace Behc.Mvp.Presenters
{
    // this presenter is meant to be used with standard LayoutGroup
    // expected to have only a few items at once
    public class DataCollectionSimplePresenter : DataPresenterBase<ReactiveModel>
    {
#pragma warning disable CS0649
        [SerializeField] private RectTransform _insertAfter;
#pragma warning restore CS0649

        private class ItemDesc
        {
            public object Model;
            public IPresenter Presenter;
            public bool Active;
        }

        private readonly List<ItemDesc> _itemPresenters = new List<ItemDesc>(16);

        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);

            UpdateContent();
        }

        public override void Unbind()
        {
            foreach (ItemDesc item in _itemPresenters)
            {
                BindingHelper.Unbind(item.Model, item.Presenter);
                _presenterMap.DestroyPresenter(item.Model, item.Presenter);
            }

            _itemPresenters.Clear();

            base.Unbind();
        }

        protected override void OnActivate()
        {
            foreach (ItemDesc item in _itemPresenters)
            {
                item.Presenter.Activate();
                item.Active = true;
            }
        }

        protected override void OnDeactivate()
        {
            foreach (ItemDesc item in _itemPresenters)
            {
                if (item.Active)
                {
                    item.Presenter.Deactivate();
                    item.Active = false;
                }
            }
        }

        protected override void OnScheduledUpdate()
        {
            if (_contentChanged)
            {
                UpdateContent();
                _contentChanged = false;
            }
        }

        private void UpdateContent()
        {
            IDataCollection dataCollection = (IDataCollection)_model;

            int insertOffset = 0;
            if (_insertAfter != null)
            {
                insertOffset = _insertAfter.GetSiblingIndex() + 1;
            }

            int index = 0;
            while (index < _itemPresenters.Count)
            {
                ItemDesc item = _itemPresenters[index];
                if (dataCollection.Data.Contains(item.Model))
                {
                    index++;
                    continue;
                }

                if (item.Active)
                {
                    item.Presenter.Deactivate();
                }

                BindingHelper.Unbind(item.Model, item.Presenter);
                _presenterMap.DestroyPresenter(item.Model, item.Presenter);

                _itemPresenters.RemoveAt(index);
            }

            index = 0;
            foreach (object itemModel in dataCollection.Data)
            {
                ItemDesc newItem;
                if (index >= _itemPresenters.Count)
                {
                    newItem = new ItemDesc
                    {
                        Model = itemModel,
                        Presenter = _presenterMap.CreatePresenter(itemModel, RectTransform),
                    };

                    BindingHelper.Bind(newItem.Model, newItem.Presenter, this, false);
                    if (IsActive)
                    {
                        newItem.Presenter.Activate();
                        newItem.Active = true;
                    }

                    _itemPresenters.Add(newItem);
                    newItem.Presenter.RectTransform.SetSiblingIndex(index + insertOffset);
                    index++;
                    continue;
                }

                if (itemModel == _itemPresenters[index].Model)
                {
                    index++;
                    continue;
                }

                int other = _itemPresenters.FindIndex(index, d => d.Model == itemModel);
                if (other >= 0)
                {
                    (_itemPresenters[index], _itemPresenters[other]) = (_itemPresenters[other], _itemPresenters[index]);
                    _itemPresenters[index].Presenter.RectTransform.SetSiblingIndex(index + insertOffset);
                    _itemPresenters[other].Presenter.RectTransform.SetSiblingIndex(other + insertOffset);
                    index++;
                    continue;
                }

                newItem = new ItemDesc
                {
                    Model = itemModel,
                    Presenter = _presenterMap.CreatePresenter(itemModel, RectTransform),
                };

                BindingHelper.Bind(newItem.Model, newItem.Presenter, this, false);
                if (IsActive)
                {
                    newItem.Presenter.Activate();
                    newItem.Active = true;
                }

                _itemPresenters.Insert(index, newItem);
                newItem.Presenter.RectTransform.SetSiblingIndex(index + insertOffset);
                index++;
            }

            RectTransform.ForceUpdateRectTransforms();
        }
    }
}