namespace Behc.Navigation
{
    public interface INavigable
    {
        void StartUp(object context, string fromName);

        object ValidateContext(object context);
        object ResolveContext(string parameters);
    }
}