# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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