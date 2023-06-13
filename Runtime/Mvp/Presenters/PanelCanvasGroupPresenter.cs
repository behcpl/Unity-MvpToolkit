using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public class PanelCanvasGroupPresenter<T> : PanelPresenterBase<T> where T : class
    {
#pragma warning disable CS0649
        [SerializeField] protected CanvasGroup _canvasGroup;
#pragma warning restore CS0649

        public sealed override void Initialize(IPresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);

            _canvasGroup.interactable = false;
        }

        public sealed override void Activate()
        {
            base.Activate();

            _canvasGroup.interactable = true;
        }

        public sealed override void Deactivate()
        {
            _canvasGroup.interactable = false;

            base.Deactivate();
        }
    }
}