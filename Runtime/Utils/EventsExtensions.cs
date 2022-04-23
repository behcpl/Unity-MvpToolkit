using System;
using Behc.Utils;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace Behc.Mvp.Utils
{
    public static class EventsExtensions
    {
        [MustUseReturnValue]
        public static IDisposable Subscribe(this UnityEvent e, UnityAction a)
        {
            e.AddListener(a);
            return new GenericDisposable(() => e.RemoveListener(a));
        }

        [MustUseReturnValue]
        public static IDisposable Subscribe<T0>(this UnityEvent<T0> e, UnityAction<T0> a)
        {
            e.AddListener(a);
            return new GenericDisposable(() => e.RemoveListener(a));
        }

        [MustUseReturnValue]
        public static IDisposable Subscribe<T0, T1>(this UnityEvent<T0, T1> e, UnityAction<T0, T1> a)
        {
            e.AddListener(a);
            return new GenericDisposable(() => e.RemoveListener(a));
        }

        [MustUseReturnValue]
        public static IDisposable Subscribe<T0, T1, T2>(this UnityEvent<T0, T1, T2> e, UnityAction<T0, T1, T2> a)
        {
            e.AddListener(a);
            return new GenericDisposable(() => e.RemoveListener(a));
        }

        [MustUseReturnValue]
        public static IDisposable Subscribe<T0, T1, T2, T3>(this UnityEvent<T0, T1, T2, T3> e, UnityAction<T0, T1, T2, T3> a)
        {
            e.AddListener(a);
            return new GenericDisposable(() => e.RemoveListener(a));
        }
    }
}