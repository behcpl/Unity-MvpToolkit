using Behc.Mvp.Model;
using Behc.Mvp.Presenter;

namespace Behc.Mvp.Utils
{
    public static class BindingHelper
    {
        public static void Bind(object model, IPresenter presenter, IPresenter parent, bool prepareForAnimation)
        {
            if (presenter == null)
                return;
            
            if(model is ITrackable trackable)
                trackable.Acquire();
                
            presenter.Bind(model, parent, prepareForAnimation);
        }
        
        public static void Unbind(object model, IPresenter presenter)
        {
            if (presenter == null)
                return;
            
            if(model is ITrackable trackable)
                trackable.Release();
                
            presenter.Unbind();
        }
    }
}