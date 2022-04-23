using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    [CreateAssetMenu(fileName = "GridLayoutOptions", menuName = "MvpToolkit/GridLayoutOptions", order = 300)]
    public class GridLayoutOptions : CollectionLayoutOptions
    {
        public enum OriginType
        {
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_RIGHT,
        }

        public enum OrderType
        {
            ROW_MAJOR, //fill rows first, expands width
            COLUMN_MAJOR, //fill columns first, expands height
        }

        public enum AlignType
        {
            ORIGIN,
            CENTER,
            OPPOSITE,
            EXPAND_GAP,
            EXPAND_ELEMENT,
        }

        public float GapWidth = 10;
        public float GapHeight = 10;
        public float ElementWidth = 100;
        public float ElementHeight = 100;
        public RectOffset Padding = new RectOffset();
        public OriginType Origin;
        public OrderType Order;
        public AlignType Align;
        public int MajorElementsMin; //defaults to 0
        public int MajorElementsMax; //defaults to 0 - no upper limit

        public override ICollectionLayout CreateLayout()
        {
            return new GridLayout(this);
        }
    }
}