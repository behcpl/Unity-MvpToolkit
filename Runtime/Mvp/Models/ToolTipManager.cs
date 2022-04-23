using System.Collections.Generic;

namespace Behc.Mvp.Models
{
    public class ToolTipManager : ReactiveModel, IToolTipManager
    {
        public object CurrentToolTip { get; private set; }
        public bool IsSuppressed => _suppressedBy.Count > 0;
        
        private readonly HashSet<object> _suppressedBy = new HashSet<object>();
        
        public void Suppress(object owner)
        {
            _suppressedBy.Add(owner);
        }

        public void ReleaseSuppression(object owner)
        {
            _suppressedBy.Remove(owner);
        }

        public void SetCurrentToolTip(object toolTip)
        {
            if (CurrentToolTip == toolTip)
                return;
            
            CurrentToolTip = toolTip;
            NotifyChanges();
        }
    }
}