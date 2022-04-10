using System;
using System.Collections.Generic;

namespace Behc.Navigation
{
    public interface INavigable
    {
        void StartUp(object parameters, out object context);
        void Resume(object parameters, object context);
        void UpdateParameters(object parameters, object context);
        void Pause(object context);
        void TearDown(object context, Action onComplete);
    }
}