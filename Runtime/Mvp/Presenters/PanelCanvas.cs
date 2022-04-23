using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Behc.Mvp.Presenters
{
    public class PanelCanvas<T> : PanelBase<T>, IPresenterSorting where T : class
    {
#pragma warning disable CS0649
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected GraphicRaycaster _graphicRaycaster;
#pragma warning restore CS0649

        public override void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);
            
            _graphicRaycaster.enabled = false;
        }

        public override void Activate()
        {
            base.Activate();

            _graphicRaycaster.enabled = true;
        }

        public override void Deactivate()
        {
            _graphicRaycaster.enabled = false;

            base.Deactivate();
        }

        public virtual void SetSortingOrder(int baseSortingOrder, int sortingLayerId)
        {
            Assert.IsTrue(gameObject.activeInHierarchy, $"Cannot override sorting for inactive {gameObject.name} canvas!");
        
            _canvas.overrideSorting = true;
            _canvas.sortingLayerID = sortingLayerId;
            _canvas.sortingOrder = baseSortingOrder;
        }
    }
}