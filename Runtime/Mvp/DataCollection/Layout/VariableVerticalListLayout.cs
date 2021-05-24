using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.DataCollection.Layout
{
    public class VariableVerticalListLayout : ICollectionLayout
    {
        private readonly ListLayoutOptions _layoutOptions;

        private float _width;

        public VariableVerticalListLayout(ListLayoutOptions layoutOptions)
        {
            _layoutOptions = layoutOptions;
        }

        public void SetContentSize(float width, float height)
        {
            _width = width;
        }

        public Rect EvaluateRect(int index, IReadOnlyList<Rect> rects, Vector2 requestedSize, float requestedGap)
        {
            Rect lastRect = index > 0 ? rects[index - 1] : new Rect(_layoutOptions.Padding.left, _layoutOptions.Padding.top, _width - _layoutOptions.Padding.horizontal, 0);

            float gap = requestedGap >= 0 ? requestedGap : _layoutOptions.GapSize;
            if (index <= 0) gap = 0;
            float size = requestedSize.y > 0 ? requestedSize.y : _layoutOptions.ElementSize;

            return new Rect(_layoutOptions.Padding.left, lastRect.yMax + gap, _width - _layoutOptions.Padding.horizontal, size);
        }

        public Vector2 GetApproximatedContentSize(Vector2 defaultSize, int itemsCount)
        {
            return new Vector2(defaultSize.x, itemsCount * _layoutOptions.ElementSize + Mathf.Max(0, itemsCount - 1) * _layoutOptions.GapSize + _layoutOptions.Padding.vertical);
        }

        public Vector2 GetOptimalContentSize(Vector2 defaultSize, IReadOnlyList<Rect> rects)
        {
            return rects.Count == 0 ? new Vector2(defaultSize.x, _layoutOptions.Padding.vertical) : new Vector2(defaultSize.x, rects[rects.Count - 1].yMax + _layoutOptions.Padding.bottom);
        }

        public bool RebuildRequired(bool widthChanged, bool heightChanged)
        {
            return widthChanged;
        }
    }
}