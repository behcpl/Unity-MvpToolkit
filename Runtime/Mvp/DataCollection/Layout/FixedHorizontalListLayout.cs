using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.DataCollection.Layout
{
    public class FixedHorizontalListLayout : ICollectionLayout
    {
        private readonly ListLayoutOptions _layoutOptions;

        private float _height;

        public FixedHorizontalListLayout(ListLayoutOptions layoutOptions)
        {
            _layoutOptions = layoutOptions;
        }

        public void SetContentSize(float width, float height)
        {
            _height = height;
        }

        public Rect EvaluateRect(int index, IReadOnlyList<Rect> rects, Vector2 requestedSize, float requestedGap)
        {
            float offset = index * (_layoutOptions.ElementSize + _layoutOptions.GapSize) + _layoutOptions.Padding.left;

            return new Rect(offset, _layoutOptions.Padding.top, _layoutOptions.ElementSize, _height - _layoutOptions.Padding.vertical);
        }

        public Vector2 GetApproximatedContentSize(Vector2 defaultSize, int itemsCount)
        {
            return new Vector2(itemsCount * _layoutOptions.ElementSize + Mathf.Max(0, itemsCount - 1) * _layoutOptions.GapSize + _layoutOptions.Padding.horizontal, defaultSize.y);
        }

        public Vector2 GetOptimalContentSize(Vector2 defaultSize, IReadOnlyList<Rect> rects)
        {
            return GetApproximatedContentSize(defaultSize, rects.Count);
        }

        public bool RebuildRequired(bool widthChanged, bool heightChanged)
        {
            return heightChanged;
        }
    }
}