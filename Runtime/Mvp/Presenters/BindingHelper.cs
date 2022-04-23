using Behc.Mvp.Models;

namespace Behc.Mvp.Presenters
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