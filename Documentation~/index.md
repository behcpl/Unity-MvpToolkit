## About MvpToolkit

This project started as few utility elements for MVP (Model-View-Presenter) architecture, adapted for Unity needs.
Now, it contains multiple components that extends beyond this one purpose, but all contributes to the main goal: help with game/ui architecture setup.
I kept the name, but it's more Minimal-Viable-Product-Toolkit now.

## Components

### Mvp

Main component, contains all classes for data presentation.

### Configuration

Optional component to standarize process of registration new elements when scene is loaded.

### Navigation

Application side naviation.

### MiniDi

Very simple dependency injection container implementation. There is no reflection/attribute magic, you need to explicitly resolve type in configurator/factory and pass it to constructor (or some Inject method for monobehaviors)

### MiniTween

Very simple tweening engine, created mostly because there is no official DOTween UPM package to depend on.


## Getting started

Minimal hello-world style game, requires a scene with PresenterKernel, DataSlotPresenter and some panel to actually present something to user.

    // HelloWorldSceneRoot.cs
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private PresenterUpdateKernel _updateKernel;
        [SerializeField] private RectTransform _container; //for inactive panels
        [SerializeField] private HelloWorldPresenter _helloWorldPresenter;

        private readonly PresenterMap _rootPresenterMap = new PresenterMap(null);
        private readonly DataSlot _mainPanel = new DataSlot();
    
        private readonly HelloWorld _hellowWorld = new HelloWorld();

        private void Start()
        {
            // initialize some root element, so we can use it to present our own data
            _updateKernel.InitializePresenters(kernel =>
            {
                _mainPanelPresenter.Initialize(_rootPresenterMap, kernel);
                _mainPanelPresenter.Bind(_mainPanel, null, false);
                _mainPanelPresenter.Activate();
            }

            // link data of type HelloWorld to use HelloWorldPresenter as a single object that is already in scene
            _rootPresenterMap.RegisterInstance<HelloWorld>(_helloWorldPresenter.gameObject, _container, _updateKernel);

            _mainPanel.Data = _hellowWorld;
        }
    }

    // HelloWorld.cs
    public class HelloWorld {}
    
    // HelloWorldPresenter.cs
    public class HelloWorldPresenter : PanelBase<HelloWorld> {}