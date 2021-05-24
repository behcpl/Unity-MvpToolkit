using UnityEngine;

namespace Behc.MiniTween
{
    [CreateAssetMenu(fileName = "MiniTweenOptions", menuName = "MvpToolkit/MiniTween", order = 500)]
    public class MiniTweenProvider : TweenProvider
    {
        public bool UnscaledTime;

        private MiniTweenKernel _kernel;
        
        public override ITweenSystem GetTweenSystem()
        {
            if (_kernel == null)
            {
                Debug.LogWarning("Creating new MiniTween kernel!");
                GameObject go = new GameObject("MiniTween");
                _kernel = go.AddComponent<MiniTweenKernel>();
                _kernel.Initialize(this);
                DontDestroyOnLoad(go);
            }
            
            return _kernel;
        }
    }
}