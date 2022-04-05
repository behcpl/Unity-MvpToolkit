using System.Collections.Generic;

namespace Behc.Utils
{
    public class TickerManager
    {
        private readonly List<ITickable> _tickables = new List<ITickable>(4);
        private readonly List<ITickable> _added = new List<ITickable>(4);
        private readonly List<ITickable> _removed = new List<ITickable>(4);

        public void Track(ITickable tickable)
        {
            _added.Add(tickable);
            _removed.Remove(tickable);
        } 
        
        public void Untrack(ITickable tickable)
        {
            _added.Remove(tickable);
            _removed.Add(tickable);
        }

        public void Update()
        {
            foreach (ITickable tickable in _removed)
            {
                _tickables.Remove(tickable);
            }
            _removed.Clear();
            
            foreach (ITickable tickable in _added)
            {
                _tickables.Add(tickable);
            }
            _added.Clear();

            foreach (ITickable tickable in _tickables)
            {
                tickable.OnTick();
            }
        }
    }
}