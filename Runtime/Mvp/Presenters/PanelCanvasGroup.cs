using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public class PanelCanvasGroup<T> : PanelBase<T> where T : class
    {
#pragma warning disable CS0649
        [SerializeField] protected CanvasGroup _canvasGroup;
#pragma warning restore CS0649

        public override void Initialize(IPresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);

            _canvasGroup.interactable = false;
        }

        public override void Activate()
        {
            base.Activate();

            _canvasGroup.interactable = true;
        }

        public override void Deactivate()
        {
            _canvasGroup.interactable = false;

            base.Deactivate();
        }
    }
}