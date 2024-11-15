using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    public class VariableHorizontalListLayout : ICollectionLayout
    {
        private readonly ListLayoutOptions _layoutOptions;

        private float _height;

        public VariableHorizontalListLayout(ListLayoutOptions layoutOptions)
        {
            _layoutOptions = layoutOptions;
        }

        public void SetContentSize(float width, float height)
        {
            _height = height;
        }

        public Rect EvaluateRect(int index, IReadOnlyList<Rect> rects, Vector2 requestedSize, float requestedGap)
        {
            Rect lastRect = index > 0 ? rects[index - 1] : new Rect(_layoutOptions.Padding.left, _layoutOptions.Padding.top, 0, _height - _layoutOptions.Padding.vertical);

            float gap = requestedGap >= 0 ? requestedGap : _layoutOptions.GapSize;
            if (index <= 0) gap = 0;
            float size = requestedSize.x > 0 ? requestedSize.x : _layoutOptions.ElementSize;

            return new Rect(lastRect.xMax + gap, _layoutOptions.Padding.top, size, _height - _layoutOptions.Padding.vertical);
        }

        public Vector2 GetApproximatedContentSize(Vector2 defaultSize, int itemsCount)
        {
            return new Vector2(itemsCount * _layoutOptions.ElementSize + Mathf.Max(0, itemsCount - 1) * _layoutOptions.GapSize + _layoutOptions.Padding.horizontal, defaultSize.y);
        }

        public Vector2 GetOptimalContentSize(Vector2 defaultSize, IReadOnlyList<Rect> rects)
        {
            return rects.Count == 0 ? new Vector2(_layoutOptions.Padding.horizontal, defaultSize.y) : new Vector2(rects[rects.Count - 1].xMax + _layoutOptions.Padding.right, defaultSize.y);
        }

        public bool RebuildRequired(bool widthChanged, bool heightChanged)
        {
            return heightChanged;
        }
    }
}