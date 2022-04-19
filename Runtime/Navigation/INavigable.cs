using System;
using System.Collections.Generic;

namespace Behc.Navigation
{
    public interface INavigable
    {
        void Start();
        void Restart(object parameters);
        void Stop();

        void LongDispose(Action onComplete);
    }
}