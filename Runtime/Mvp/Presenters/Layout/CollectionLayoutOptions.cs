using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    public abstract class CollectionLayoutOptions : ScriptableObject
    {
        public abstract ICollectionLayout CreateLayout();
    }
}