using System;

namespace Behc.Utils
{
    public class WhenAll
    {
        private bool[] _conditions;
        private Action _onComplete;

        public void Setup(Action onComplete, int conditionsCount)
        {
            _onComplete = onComplete;

            if (_conditions == null || _conditions.Length != conditionsCount)
                _conditions = new bool[conditionsCount];

            for (int i = 0; i < _conditions.Length; i++)
                _conditions[i] = false;
        }

        public void Completed(int index)
        {
            _conditions[index] = true;

            for (int i = 0; i < _conditions.Length; i++)
            {
                if (!_conditions[i])
                    return;
            }

            _onComplete?.Invoke();
            _onComplete = null;
        }
    }

}