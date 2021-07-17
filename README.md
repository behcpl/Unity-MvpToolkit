# Unity-MvpToolkit
Lean architecture for rapid UI heavy games setup for Unity.
It is based around [Model-View-Presenter](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93presenter)
pattern with optional auxiliary components for typical tasks like
scene configuration or dependency management.

[Documentation](./Documentation~/index.md)

### Installation

MvpToolkit is available as a package and can be installed using Package Manager with 
direct git link or using OpenUPM. The latter is preferred as it enabled easy version updates.

In order to use OpenUPM, [scoped registry](https://docs.unity3d.com/2020.3/Documentation/Manual/upm-scoped.html)
must be added to Package Manager Project Settings: *https://package.openupm.com* with *com.behc* scope.
