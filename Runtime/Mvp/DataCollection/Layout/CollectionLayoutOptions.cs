using UnityEngine;

namespace Behc.Mvp.DataCollection.Layout
{
    public abstract class CollectionLayoutOptions : ScriptableObject
    {
        public abstract ICollectionLayout CreateLayout();
    }
}