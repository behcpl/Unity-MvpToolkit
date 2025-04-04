using System;
using Behc.Utils;
using UnityEngine;

namespace Behc.MiniTween
{
    [CreateAssetMenu(fileName = "MiniTweenOptions", menuName = "MvpToolkit/MiniTween", order = 500)]
    public class MiniTweenProvider : AbstractProvider<ITweenSystem>
    {
        public bool UnscaledTime;
        public bool DisablePooling;

        [NonSerialized] private MiniTweenKernel _kernel;

        public override ITweenSystem GetInstance()
        {
            if (_kernel.IsNull())
            {
#if BEHC_MVPTOOLKIT_VERBOSE
                Debug.LogWarning("Creating new MiniTween kernel!");
#endif
                GameObject go = new GameObject("MiniTween");
                _kernel = go.AddComponent<MiniTweenKernel>();
                _kernel.Initialize(this);
                DontDestroyOnLoad(go);
            }

            return _kernel;
        }

#if UNITY_EDITOR
        protected override void OnEditorReset()
        {
            _kernel = null;
        }
#endif
    }
}