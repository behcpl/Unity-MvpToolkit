namespace Behc.Configuration
{
    public interface IConfigurator
    {
        void Load(IDependencyResolver resolver);
        void Unload(IDependencyResolver resolver);
    }
}