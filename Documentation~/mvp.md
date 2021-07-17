### Model

Simple class, can by anything

IReactive - optional, can use UniRx or events instead
ITrackable - for models that need to know if they are being used by some presenters

### View

Passive View: Prefabs, collection of components

### Presenter

MonoBehavior that implements IPresenter

    PanelBase<T>, PanelCanvas<T>, PanelCanvasGroup<T> for user code

IPresenter lifecycle

DisposeOnUnbind/Destroy, event Subscibe()

    PanelXXX<T> lifecycle

### PresenterMap

maps model to adequate presenter, can use predicate on model data

### DataPresenters

DataPresenter is a Presenter that shows other presenter

most basic is DataSlot

### RootScene

The very first scene that will handle Mvp

Must contains some data presenters to be useful

### PresenterKernel

Keeps order of presenter update

All changes are defered