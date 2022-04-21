##MiniTween

Very basic tweening engine. Only supports simple animation, no sequences or complex parameters like looping.

Some built-in components of Mvp uses it for curtain animation or simple sliding movement.

There is no static/singleton instance. To obtain one, you need to call GetInstance() from scriptable object provider.
As long as there is only one object, returned instance will be the same.

> ⓘ You can replace implementation of scriptable object provider for testing purposes.

> ⚠️ When using addressables it is very easy to duplicate scriptable objects. Make sure MiniTweenOptions asset is marked as addressable. 
 
    public class ExamplePanelPresenter : PanelBase<ExamplePanel>
    {
        //...
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private AbstractProvider<ITweenSystem> _miniTweenProvider;

        protected override void OnBind(bool prepareForAnimation)
        {
            // initialize some stuff
            
            // set alpha to 0 for animated path
            _canvasGroup.alpha = prepareForAnimation ? 0 : 1;
        }

        protected override void OnAnimateShow(float startTime, Action onFinish)
        {
            _canvasGroup.AnimateAlpha( _miniTweenProvider.GetInstance(), 1.0f, 0.3f).SetCompleteCallback(onFinish);
        }
    }

> ⓘ You can assign provider asset for any script, so Unity editor will automatically set it in edit mode. Useful for frequently used components.