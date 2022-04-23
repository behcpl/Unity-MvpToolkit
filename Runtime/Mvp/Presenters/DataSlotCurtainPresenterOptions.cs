using UnityEngine;

namespace Behc.Mvp.Presenters
{
    [CreateAssetMenu(fileName = "DataSlotCurtainOptions", menuName = "MvpToolkit/DataSlotCurtainOptions", order = 100)]
    public class DataSlotCurtainPresenterOptions : ScriptableObject
    {
        public float ShowDuration = 0.2f;
        public float HideDuration = 0.2f;
        public AnimationCurve ShowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve HideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}