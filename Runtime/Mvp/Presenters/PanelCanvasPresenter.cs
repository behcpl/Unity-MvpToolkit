using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Behc.Mvp.Presenters
{
    public class PanelCanvasPresenter<T> : PanelPresenterBase<T>, IPresenterSorting where T : class
    {
#pragma warning disable CS0649
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected GraphicRaycaster _graphicRaycaster;
#pragma warning restore CS0649

        public sealed override void Initialize(IPresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);
            
            _graphicRaycaster.enabled = false;
        }

        public sealed override void Activate()
        {
            base.Activate();

            _graphicRaycaster.enabled = true;
        }

        public sealed override void Deactivate()
        {
            _graphicRaycaster.enabled = false;

            base.Deactivate();
        }

        public void SetSortingOrder(int baseSortingOrder, int sortingLayerId)
        {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot override sorting for inactive {gameObject.name} canvas!");
        
            _canvas.overrideSorting = true;
            _canvas.sortingLayerID = sortingLayerId;
            _canvas.sortingOrder = baseSortingOrder;

            OnSetSortingOrder(baseSortingOrder, sortingLayerId);
        }

        protected virtual void OnSetSortingOrder(int baseSortingOrder, int sortingLayerId)
        {
            //NOOP
        }
    }
}