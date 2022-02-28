namespace Behc.Navigation
{
    public abstract class NavigationBase : INavigable
    {
        public abstract void StartUp(object context, string fromName);

        public object ValidateContext(object context) => context;
        public object ResolveContext(string parameters) => null;
    }
}