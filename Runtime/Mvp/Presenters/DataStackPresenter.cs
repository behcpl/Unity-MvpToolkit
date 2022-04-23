using Behc.Mvp.Models;
using Behc.Mvp.Utils;
using Behc.Utils;

namespace Behc.Mvp.Presenters
{
    public class DataStackPresenter : AnimatedPresenterBase<DataStack>
    {
        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);

            if (_curtain.IsNotNull())
            {
                DisposeOnUnbind(_curtain.OnTrigger.Subscribe(CurtainClicked));

                if (prepareForAnimation && _items.Count > 0)
                {
                    _curtain.Show(_items.Count * _sortingStep);
                }
                else
                {
                    _curtain.Setup(_items.Count > 0, _items.Count * _sortingStep);
                }
            }
        }

        protected override void OnActivate()
        {
            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.State == ItemState.READY)
                {
                    topLevel.Presenter.Activate();
                    topLevel.Active = true;
                }
            }
        }

        protected override void OnDeactivate()
        {
            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.Active)
                {
                    topLevel.Presenter.Deactivate();
                    topLevel.Active = false;
                }
            }
        }

        protected override void UpdateContent()
        {
            int oldCount = _items.Count;
            base.UpdateContent();

            for (int index = 0; index < _items.Count - 1; index++)
            {
                ItemDesc desc = _items[index];
                if (desc.Active)
                {
                    desc.Presenter.Deactivate();
                    desc.Active = false;
                }
            }

            if (_items.Count > 0 && IsActive)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (!topLevel.Active && topLevel.State == ItemState.READY)
                {
                    topLevel.Presenter.Activate();
                    topLevel.Active = true;
                }
            }

            if (_curtain.IsNotNull())
            {
                if (_items.Count == 0)
                {
                    if (oldCount > 0)
                        _curtain.Hide();
                }
                else
                {
                    if (oldCount == 0)
                        _curtain.Show(_items.Count * _sortingStep);
                    else if (oldCount != _items.Count)
                        _curtain.Switch(_items.Count * _sortingStep, oldCount * _sortingStep);
                }
            }
        }

        private void CurtainClicked()
        {
            _model.TryRemoveTopLevel();
        }
    }
}