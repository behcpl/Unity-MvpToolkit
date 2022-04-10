namespace Behc.Configuration
{
    public interface IDependencyResolver
    {
        T Resolve<T>() where T : class;
        T Resolve<T>(string name) where T : class;
    }
}