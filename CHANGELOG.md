# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.9.3] - 2023-03-20
### Breaking Changes
- Removed local PresenterMap from presenter factories (can be declared explicitly)
### Added
- Support for entering Play Mode without Domain Reload
### Bugfixes
- Fixed animations completed before starting not setting final values

## [0.9.2] - 2022-09-10
### Changes
- PresenterMap: removed RegisterOverride (does not work with modding scenarios)
### Added
- DisposeOnDeactivate added to PanelBase
### Bugfixes
- Fixed markdown formatting errors

## [0.9.1] - 2022-09-10
### Bugfixes
- Use expensive null-check for some SerializeFields

## [0.9.0] - 2022-09-10
### Added
- PresenterMap: ability to RegisterOverride (required for modding support)
- IPresenterFactory implementations: creating and exposing local PresenterMap
### Changes
- PresenterMap: default factory (without predicate) is always returned last, predicates are still register order dependent

## [0.8.1] - 2022-04-24
### Breaking Changes
- Reorder presenter factories parameters
- Tooltip provider can return different tooltip models depending on hit GameObject 
### Added
- Expanded IInjectable (and factories) to 8 parameters
- Added more MiniTween extensions
- New simple collection presenter for better interoperability with UGUI layout groups
 
## [0.8.0] - 2022-04-24
### Breaking Changes
- Reworked configuration
  - removed TContext in favor of IDependencyResolver
  - common loader (not limited to scenes)
- Reworked navigation
  - introduce factory concept to navigation elements
- Reorganized folder structure for Mvp part
### Added
- IFactory - generic factory interface
- ITickable, TickerManager - abstracted Update() loop for systems
- AbstractProvider\<T\> - as scriptable object backed service provider
- various annotations
- Addressables support for configuration

## [0.7.0] - 2022-03-05
### Bugfixes
- DataCollectionAnimatedPresenter fixes:
  - fixed OnDrawGizmosSelected exception while not in play mode
  - fixed exceptions when aborting show/hide animations
  - fixed onFinish never called when collection was empty
### Breaking Changes
- Replaced UndoStack with NavigationManager
- Removed mandatory 'Id' from DataCollection items
  - now the reference alone determines if item is the same
  - no need for Rebind anymore
- Removed NotifyChanges from IReactive
- Moved stack options from presenter to model
### Changes
- DataSlot(...)Presenter family no longer requires concrete model class
- DataCollection(...)Presenter family no longer requires model derived from specific abtract class
### Added
- Blocking support, some presenter might be blocked from activation or block others
- BackButtonManager for better control
- DisposableExtensions
- IDataSlot for any compatible DataSlot models

## [0.6.2] - 2021-06-06
### Bugfixes
- Fixed ButtonEx initial state
- Fixed MiniTween skipping setter callback on complete
- Fixed DataCollectionAnimatedPresenter animation bugs
- Fixed Panels not starting deactivated
### Breaking Changes
- Improved Utils folder consistency
- Moved MiniTween extensions to separate namespace
### Changes
- Optimized null-checks for Unity Object types (skipping native object check when not needed)
### Added
- More MiniTween animating extensions

## [0.6.1] - 2021-06-06
### Bugfixes
- Fixed missing using for InputModule path

## [0.6.0] - 2021-06-05
### Added
- Simple Game Sample - shows basic navigation
- More examples to Data Presenter Gallery Sample
### Breaking Changes
- Reworked how popup menu manager works, now is more in line with the rest of components
### Bugfixes
- Fixed invalid curtain animation
- Fixed PoolableInjectableFactory initialization order
- Fixed ButtonEx longpress

## [0.5.0] - 2021-05-24
### Added
- Initial release