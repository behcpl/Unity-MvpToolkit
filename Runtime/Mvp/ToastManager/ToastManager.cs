using System.Collections.Generic;
using Behc.Mvp.Model;

namespace Behc.Mvp.ToastManager
{
    public class ToastManager : ReactiveModel, IToastManager
    {
        public IReadOnlyList<object> Toasts => _toasts;

        private readonly List<object> _toasts = new List<object>(16);

        public void AddToast(object toastData)
        {
            _toasts.Add(toastData);
            NotifyChanges();
        }

        public void FinishToast(object toastData)
        {
            _toasts.Remove(toastData);
            NotifyChanges();
        }

        public void ClearAllToasts()
        {
            _toasts.Clear();
            NotifyChanges();
        }
    }
}