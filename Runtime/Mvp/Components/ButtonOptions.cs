using UnityEngine;

namespace Behc.Mvp.Components
{
    [CreateAssetMenu(fileName = "ButtonOptions", menuName = "MvpToolkit/ButtonOptions", order = 600)]
    public class ButtonOptions : ScriptableObject
    {
        public float HoverDelay = 0.2f;
        public float LongPressDuration = 0.5f;
    }
}
