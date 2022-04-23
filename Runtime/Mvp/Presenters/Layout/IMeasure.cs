using UnityEngine;

namespace Behc.Mvp.Presenters.Layout
{
    public interface IMeasure
    {
        void Prepare(Vector2 size);
        Vector2 MeasureModel(object model);
    }
}