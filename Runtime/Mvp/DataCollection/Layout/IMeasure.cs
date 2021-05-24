using UnityEngine;

namespace Behc.Mvp.DataCollection.Layout
{
    public interface IMeasure
    {
        void Prepare(Vector2 size);
        Vector2 MeasureModel(object model);
    }
}