using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    public interface ICollectionLayout
    {
        void SetContentSize(float width, float height);
        
        Rect EvaluateRect(int index, IReadOnlyList<Rect> rects, Vector2 requestedSize, float requestedGap);
        Vector2 GetApproximatedContentSize(Vector2 defaultSize, int itemsCount);
        Vector2 GetOptimalContentSize(Vector2 defaultSize, IReadOnlyList<Rect> rects);

        bool RebuildRequired(bool widthChanged, bool heightChanged);
    }
}