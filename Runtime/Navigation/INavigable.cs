namespace Behc.Navigation
{
    public interface INavigable
    {
        void StartUp(object context);
        void Reset(object context);
        void TearDown();
        object ResolveContext(string parameters);
    }
}