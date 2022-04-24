# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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