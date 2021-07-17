using System;

namespace Behc.Utils
{
    public struct WhenBoth
    {
        private bool _first;
        private bool _second;
        private Action _onComplete;

        public void Setup(Action onComplete)
        {
            _onComplete = onComplete;
        }

        public void CompletedFirst()
        {
            _first = true;
            if (_second)
            {
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }

        public void CompletedSecond()
        {
            _second = true;
            if (_first)
            {
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }
    }

  }