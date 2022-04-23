using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    [CreateAssetMenu(fileName = "ListLayoutOptions", menuName = "MvpToolkit/ListLayoutOptions", order = 300)]
    public class ListLayoutOptions : CollectionLayoutOptions
    {
        public enum DirectionType
        {
            VERTICAL,
            HORIZONTAL,
        }

        public RectOffset Padding = new RectOffset();
        public DirectionType Direction;
    
        public float GapSize = 10;
        public float ElementSize = 100;

        public bool FixedElementSize = true;
        
        public override ICollectionLayout CreateLayout()
        {
            return Direction == DirectionType.HORIZONTAL ? 
                FixedElementSize ? (ICollectionLayout) new FixedHorizontalListLayout(this) : new VariableHorizontalListLayout(this) : 
                FixedElementSize ? (ICollectionLayout) new FixedVerticalListLayout(this) : new VariableVerticalListLayout(this);
        }
    }
}