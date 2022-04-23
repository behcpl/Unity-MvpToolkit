using UnityEngine;

namespace Behc.Mvp.Presenters
{
    [CreateAssetMenu(fileName = "DataSlotSlidePresenterOptions", menuName = "MvpToolkit/DataSlotSlideOptions", order = 100)]
    public class DataSlotSlidePresenterOptions : ScriptableObject
    {
        public enum DirectionType
        {
            LEFT_RIGHT,
            RIGHT_LEFT,
            TOP_DOWN,
            BOTTOM_UP,
        }

        public DirectionType Direction;
        public float TransitionDuration = 0.2f;
        public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}