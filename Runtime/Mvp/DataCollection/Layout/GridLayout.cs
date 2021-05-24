using System.Collections.Generic;
using UnityEngine;

namespace Behc.Mvp.DataCollection.Layout
{
    public class GridLayout : ICollectionLayout
    {
        private readonly GridLayoutOptions _options;

        private float _width;
        private float _height;

        private float _majorSize = 0;
        private float _majorElementSize = 0;
        private float _majorGap = 0;
        private int _elementsPerMajor = 0;

        public GridLayout(GridLayoutOptions options)
        {
            _options = options;
        }

        public void SetContentSize(float width, float height)
        {
            _width = width;
            _height = height;

            UpdateSizes(width, height, _options, out _majorSize, out _majorElementSize, out _majorGap, out _elementsPerMajor);
        }

        public Rect EvaluateRect(int index, IReadOnlyList<Rect> rects, Vector2 requestedSize, float requestedGap)
        {
            float eleH, eleW, gapH, gapW;
            int row, col;
            float offX = 0;
            float offY = 0;
            float offset = _majorSize - (_majorElementSize * _elementsPerMajor + _majorGap * (_elementsPerMajor - 1));

            if (_options.Order == GridLayoutOptions.OrderType.ROW_MAJOR)
            {
                col = index % _elementsPerMajor;
                row = index / _elementsPerMajor;
                gapW = _majorGap;
                gapH = _options.GapHeight;
                eleW = _majorElementSize;
                eleH = _options.ElementHeight;

                if (_options.Align == GridLayoutOptions.AlignType.CENTER)
                    offX = (offset - _options.Padding.horizontal) * 0.5f;
                if (_options.Align == GridLayoutOptions.AlignType.OPPOSITE)
                    offX = offset - _options.Padding.horizontal;
            }
            else
            {
                col = index / _elementsPerMajor;
                row = index % _elementsPerMajor;
                gapW = _options.GapWidth;
                gapH = _majorGap;
                eleW = _options.ElementWidth;
                eleH = _majorElementSize;

                if (_options.Align == GridLayoutOptions.AlignType.CENTER)
                    offY = (offset - _options.Padding.vertical) * 0.5f;
                if (_options.Align == GridLayoutOptions.AlignType.OPPOSITE)
                    offY = offset - _options.Padding.vertical;
            }

            float offsetX = col * (eleW + gapW) + offX;
            float offsetY = row * (eleH + gapH) + offY;

            if (_options.Origin == GridLayoutOptions.OriginType.BOTTOM_LEFT || _options.Origin == GridLayoutOptions.OriginType.BOTTOM_RIGHT)
            {
                offsetY = _height - offsetY - _options.Padding.bottom - eleH;
            }
            else
            {
                offsetY += _options.Padding.top;
            }

            if (_options.Origin == GridLayoutOptions.OriginType.TOP_RIGHT || _options.Origin == GridLayoutOptions.OriginType.BOTTOM_RIGHT)
            {
                offsetX = _width - offsetX - _options.Padding.right - eleW;
            }
            else
            {
                offsetX += _options.Padding.left;
            }

            return new Rect(offsetX, offsetY, eleW, eleH);
        }

        public Vector2 GetApproximatedContentSize(Vector2 defaultSize, int itemsCount)
        {
            UpdateSizes(defaultSize.x, defaultSize.y, _options, out float majorSize, out float majorElementSize, out float majorGap, out int elementsPerMajor);

            if (_options.Order == GridLayoutOptions.OrderType.ROW_MAJOR)
            {
                float width = majorSize;

                int rows = (itemsCount + elementsPerMajor - 1) / elementsPerMajor;
                float height = rows * _options.ElementHeight + Mathf.Max(0, rows - 1) * _options.GapHeight + _options.Padding.vertical;

                return new Vector2(width, height);
            }
            else
            {
                float height = majorSize;

                int columns = (itemsCount + elementsPerMajor - 1) / elementsPerMajor;
                float width = columns * _options.ElementWidth + Mathf.Max(0, columns - 1) * _options.GapWidth + _options.Padding.horizontal;

                return new Vector2(width, height);
            }
        }

        public Vector2 GetOptimalContentSize(Vector2 defaultSize, IReadOnlyList<Rect> rects)
        {
            return GetApproximatedContentSize(defaultSize, rects.Count);
        }

        public bool RebuildRequired(bool widthChanged, bool heightChanged)
        {
            return widthChanged || heightChanged; //TODO: check row/col constraints?
        }

        private static void UpdateSizes(float proposedWidth, float proposedHeight, GridLayoutOptions options, out float majorSize, out float majorElementSize, out float majorGap, out int elementsPerMajor)
        {
            float elementSize, margins;

            if (options.Order == GridLayoutOptions.OrderType.ROW_MAJOR)
            {
                majorSize = proposedWidth;
                elementSize = options.ElementWidth;
                majorElementSize = options.ElementWidth;
                majorGap = options.GapWidth;
                margins = options.Padding.horizontal;
            }
            else
            {
                majorSize = proposedHeight;
                elementSize = options.ElementHeight;
                majorElementSize = options.ElementHeight;
                majorGap = options.GapHeight;
                margins = options.Padding.vertical;
            }

            elementsPerMajor = 1 + Mathf.FloorToInt((majorSize - margins - elementSize) / (elementSize + majorGap));

            int lowerBound = options.MajorElementsMax > 0 ? options.MajorElementsMin : 0;
            int upperBound = Mathf.Max(lowerBound, options.MajorElementsMax > 0 ? options.MajorElementsMax : int.MaxValue);
            elementsPerMajor = Mathf.Clamp(elementsPerMajor, options.MajorElementsMin, upperBound);

            if (options.Align == GridLayoutOptions.AlignType.EXPAND_GAP && elementsPerMajor > 1)
            {
                majorGap = (majorSize - margins - elementsPerMajor * elementSize) / (elementsPerMajor - 1);
            }

            if (options.Align == GridLayoutOptions.AlignType.EXPAND_ELEMENT)
            {
                if (elementsPerMajor > 1)
                    majorElementSize = (majorSize - margins - majorGap * (elementsPerMajor - 1)) / elementsPerMajor;
                else
                    majorElementSize = majorSize - margins;
            }
        }
    }
}