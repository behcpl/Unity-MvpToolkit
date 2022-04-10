using System;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public interface INavigationRegistry
    {
        [MustUseReturnValue]
        IDisposable Register([NotNull] string name, [NotNull] INavigable navigable);

        INavigable Get([NotNull] string name);
    }
}