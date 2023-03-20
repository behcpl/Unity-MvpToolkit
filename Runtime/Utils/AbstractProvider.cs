using UnityEngine;

namespace Behc.Utils
{
    // [CreateAssetMenu(fileName = "AbstractProvider", menuName = "Game/Service/Provider", order = 0)]
    public abstract class AbstractProvider<TService> : ScriptableObject
    {
        public abstract TService GetInstance();

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }

        private void EditorApplicationOnplayModeStateChanged(UnityEditor.PlayModeStateChange obj)
        {
            if (obj == UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                OnEditorReset();
            }
        }

        protected virtual void OnEditorReset() { }
#endif
    }
}