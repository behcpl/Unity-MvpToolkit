using UnityEngine;

namespace Behc.Mvp.DataCollection
{
    [CreateAssetMenu(fileName = "DataCollectionAnimatedOptions", menuName = "MvpToolkit/DataCollectionAnimatedOptions", order = 100)]
    public class DataCollectionAnimatedOptions : ScriptableObject
    {
        [Header("Show")] public float ShowDuration = 0.2f;
        public Vector2 ShowOffset = Vector2.zero;
        public Vector2 ShowWeight = Vector2.zero;
        public AnimationCurve ShowCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float ShowPropagateDelay = 0;

        [Header("Hide")] public float HideDuration = 0.2f;
        public Vector2 HideOffset = Vector2.zero;
        public Vector2 HideWeight = Vector2.zero;
        public AnimationCurve HideCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float HidePropagateDelay = 0;

        [Header("Move")] public float MoveDuration = 0.2f;
        public AnimationCurve MoveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Delays")] public float MoveAfterHideDelay = 0;
        public float ShowAfterHideDelay = 0;
        public float ShowAfterMoveDelay = 0;
    }
}