using System;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public interface INavigationRegistry : IFactory<string, object, INavigable>
    {
        [MustUseReturnValue]
        IDisposable Register([NotNull] string name, [NotNull] IFactory<object, INavigable> navigableFactory);
    }
}