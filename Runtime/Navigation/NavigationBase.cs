namespace Behc.Navigation
{
    public abstract class NavigationBase : INavigable
    {
        public abstract void StartUp(object context);

        public virtual void Reset(object context)
        {
            TearDown();
            StartUp(context);
        }

        public virtual void TearDown() { }

        public object ResolveContext(string parameters) => null;
    }
}