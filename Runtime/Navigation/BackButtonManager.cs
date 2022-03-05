using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public class BackButtonManager
    {
        private struct Description
        {
            public int Priority;
            public Func<bool> Handler;
        }

        private readonly List<Description> _handlers = new List<Description>(16);
        
        //handler returns true if consumed
        [MustUseReturnValue]
        public IDisposable RegisterHandler(Func<bool> handler, int priority)
        {
            int insertAt = _handlers.Count;
            for (int i = 0; i < _handlers.Count; i++)
            {
                if (_handlers[i].Priority > priority)
                {
                    insertAt = i;
                    break;
                }
            }
            
            Description description = new Description { Priority = priority, Handler = handler };
            _handlers.Insert(insertAt, description);
            return new GenericDisposable(() => { _handlers.Remove(description); });
        }

        public void BackButtonClicked()
        {
            for (int i = _handlers.Count; i > 0; i--)
            {
                if (_handlers[i - 1].Handler())
                    break;
            }
        }
    }
}