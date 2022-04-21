##Configuration

*(wip)*
Configuration elements:
 - Root element on scene (additive loading)
 - ScriptableObject (for addressables)

MiniDiContainer - simplistic dependency injection system

Use pure di (by constructor) where possible, and IInjectable for MonoBehaviours
Get dependencies from context or resolve from container, but only inside configuration phase
