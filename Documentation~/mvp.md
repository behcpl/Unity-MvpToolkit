## Table of Contents
1. [Basic Types](#basic-types)
2. [Panel base](#panel-base)
3. [Data slot](#data-slot)
4. [Data collection](#data-collection)
5. [Data stack](#data-stack)
6. [Popup menu manager](#popup-menu-manager)
7. [Toasts manager](#toasts-manager)
8. [ToolTip manager](#tooltip-manager)


## Basic Types


### Model

Model is a simple class, can by anything, there is no mandatory base class or interface.

Optional interfaces:
- IReactive - Allows presenters to refresh itself, every time model has changed. You can use UniRx or events instead.
- ITrackable - Allows models to be aware if some presenter is using it. Can be useful if model requires lots of resources but can be paused.
- ICloseOptions - Interface used by DataStack to control behaviour of removing models.


### View

View is a collection of GameObjects on scene or as a prefab.
Any components od MonoBehaviours not implementing IPresenter are considered part of View.


### Presenter

Presenter is a MonoBehaviour that implements IPresenter. Check IPresenter.cs for details.

IPresenter has strict lifecycle:
- Initialize - called exactly once
- Bind - called once until Unbind call
- AnimateShow (optional) - called once after Bind
- Activate - called once until Deactivate call
- Deactivate
- AnimateHide (optional)
- Unbind
- Destroy - called exactly once

There is no need to implement IPresenter directly. Helper generic classes exists to cover for much of boilerplate code.
Check [panel base](#panel-base)


### PresenterMap

PresenterMap is a class that provides a link between model type and presenter that can be used to visualize data in that model.
There are 2 overloads of Register<T>
1. Default
2. With predicate

Registering with predicate will evaluate condition at Bind time, with initialized model.
If many presenters are registered, predicates will be evaluated in order of registrations,
so it is best to register most specific predicates first.
If no presenter is found, exception will be thrown.

Presenters are registered using IPresenterFactory, there are multiple implementations that covers most cases:
- SingleInstanceFactory - if GameObject is already on scene, and we only ever need one copy at any given time
- SimpleFactory - will create GameObject from prefab, and destroy if no longer needed
- PooledFactory - will create GameObject pool, and use it if needed
- SimpleInjectableFactory/PooledInjectableFactory - like above, but with extra dependency injection

Typical scenario includes one global PresenterMap that servers as a fallback for other presenters' PresenterMaps.
That way you can register presenters that are available in whole application, but still have an ability to override registered prefabs for specific parts.


### PresenterUpdateKernel

PresenterUpdateKernel is a mandatory MonoBehaviour that must be included on root scene.
It's best to adjust script order execution to make it update after other scripts.

All presenters ScheduledUpdate is during kernels update with strict order: parent updates before children.
On top of that, all changes to presenters are deferred until this kernel update happens.

So code like this:

    firstSlot.Data = otherSlot;
    otherSlot.Data = someModel;

does not depend on order. When scheduled update happens for firstSlot's presenter, otherSlot's presenter will follow with 'child' model set.

Also, being deferred, code like below doesn't trigger any updates (presenter is still notified, but does early exit)

    // firstSlot.Data was null
    firstSlot.Data = someModel;
    firstSlot.Data = null;


## Panel base

*(wip)*
Base implementation of IPresenter interface. Most common use case will require to inherit from PanelBase<TModel>
and override OnBind method to populate presenter with model data.
It supports composition using RegisterPresenter

    PanelBase<T>, PanelCanvas<T>, PanelCanvasGroup<T> for user code


## Data slot

*(wip)*
Most basic container. Can dynamically assign Data, that presenter will try to show.

Models: 
    
    DataSlot, DataSlot<T>


Presenters:
    
    DataSlotPresenter - basic presenter, no animations
    DataSlotCurtainPresenter - transition as crosfade to curtain
    DataSlotSlidePresenter - old data moves out, while new data moves in 

## Data collection

*(wip)*
Container with multiple children. Presenters can use various layouts like list or grid.
Presenters can be nested, i.e. list of sections, where each section has grid of items inside.

## Data stack

*(wip)*
Specialized container for data that have single top (and active) element. Most common use is for popup windows manager.

    public void Push<TResult>(object model, Action<TResult> onClose, TResult defaultResult = default, bool canDefaultClose = true)

All data pushed to stack is expected to return some value when removed.

## Popup menu manager

*(wip)*
Specialized container for popup menu structures

## Toasts manager

*(wip)*
Specialized container for toasts (short lived messages)

## ToolTip manager

*(wip)*
Specialized container for tooltips (non-interactive, on-demand elements)
