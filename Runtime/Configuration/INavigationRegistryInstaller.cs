using Behc.Navigation;

namespace Behc.Configuration
{
    public interface INavigationRegistryInstaller
    {
        public void Install(IDependencyResolver resolver, INavigationRegistry navigationRegistry);
    }

    public static class NavigationRegistryExtensions
    {
        public static INavigationRegistry Add<T>(this INavigationRegistry navigationRegistry, IDependencyResolver resolver) where T : INavigationRegistryInstaller, new()
        {
            T installer = new T();
            installer.Install(resolver, navigationRegistry);
            return navigationRegistry;
        }
    }
}