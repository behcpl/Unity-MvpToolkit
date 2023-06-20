using System;
using JetBrains.Annotations;

namespace Behc.Configuration
{
    public interface IDependencyResolver
    {
        object Resolve([NotNull] Type type, string name, bool allowUnnamed);
    }

    public static class DependencyResolverExtensions
    {
        public static T Resolve<T>(this IDependencyResolver resolver) where T : class
        {
            return resolver.Resolve(typeof(T), null, true) as T;
        }

        public static T Resolve<T>(this IDependencyResolver resolver, string name) where T : class
        {
            return resolver.Resolve(typeof(T), name, false) as T;
        }

        public static T CreateInstance<T>(this IDependencyResolver resolver) where T : class
        {
            return (T)MiniDiActivator.CreateInstance(resolver, typeof(T));
        }
        
        public static T CreateInstance<T>(this IDependencyResolver resolver, params object[] additionalParameters) where T : class
        {
            return (T)MiniDiActivator.CreateInstance(resolver, typeof(T), additionalParameters);
        }
    }
}