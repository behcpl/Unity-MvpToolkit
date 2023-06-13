namespace Behc.Mvp.Presenters
{
    public class PanelPresenter<T> : PanelPresenterBase<T> where T : class
    {   
        public sealed override void Initialize(IPresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);
        }
        
        public sealed override void Activate()
        {
            base.Activate();
        }

        public sealed override void Deactivate()
        {
            base.Deactivate();
        }
    }
}