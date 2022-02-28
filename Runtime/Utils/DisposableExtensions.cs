using System;

namespace Behc.Utils
{
    public static class DisposableExtensions
    {
        public static void KeepForever(this IDisposable _) {}
    }
}