## About MvpToolkit

This project started as few utility elements for MVP (Model-View-Presenter) architecture, adapted for Unity needs.
Now, it contains multiple components that extends beyond this one purpose, but all contributes to the main goal: help with game/ui architecture setup.
I kept the name, but it's more Minimal-Viable-Product-Toolkit now.

#### Core concepts
 - Everything can be customized/expanded/replaced
 - No external dependencies (dependencies like UniRx or Zenject can be used with MvpToolkit but are not required)
 - No globals/statics/singletons 
 
#### Recommended for
 - UI heavy games (lots of windows and panels with complex navigation between them)

#### Not recommended for
 - Games with very little UI (only main menu or some HUD)
 - Use for elements other than UI 

## Components

### Mvp (Model-View-Presenter)

Core component with all classes required for Mvp pattern and many helpers and generics
to keep boilerplate code to minimum.

[Read more](./mvp.md)

### Configuration

Optional component to streamline process of registration new elements when game (or its part) is loaded.

[Read more](./configuration.md)

### Navigation

Optional component with application side navigation.

[Read more](./navigation.md)

### MiniTween

Very simple tweening engine, created mostly because there is no official DOTween UPM package to depend on.
It is used in a few parts of Mvp component for animating transitions.

[Read more](./tween.md)

## Getting started

Minimal hello-world style game, requires a scene with PresenterKernel, DataSlotPresenter and some panel to actually present something to user.

    // HelloWorldSceneRoot.cs
    public class SampleScene : MonoBehaviour
    {
        // those elements should be added somewhere in scene
        [SerializeField] private PresenterUpdateKernel _updateKernel;
        [SerializeField] private RectTransform _container; //for inactive panels
        [SerializeField] private DataSlotPresenter _mainPanelPresenter; //top-most slot, where we can put some panels
        
        // this is example panel, normally it would be defined in different scene or available as a prefab
        [SerializeField] private HelloWorldPresenter _helloWorldPresenter;

        // this is part of root configuration, you need some top level presenters to show anything
        private readonly PresenterMap _rootPresenterMap = new PresenterMap(null);
        private readonly DataSlot _mainPanel = new DataSlot();
    
        private readonly HelloWorld _helloWorld = new HelloWorld();

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
            // this would be normally during configuration phase i.e. when scene is loaded
            _rootPresenterMap.RegisterInstance<HelloWorld>(_helloWorldPresenter.gameObject, _container, _updateKernel);

            // in this example panel is set once, but this can be changed at runtime i.e. using navigation system
            _mainPanel.Data = _helloWorld;
        }
    }

    // HelloWorld.cs
    public class HelloWorld {}
    
    // HelloWorldPresenter.cs
    public class HelloWorldPresenter : PanelBase<HelloWorld> {}

For more examples, install samples from package.
  