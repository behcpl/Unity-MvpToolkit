using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public interface IToastPresenterOptions
    {
        float LifeTime { get; }
        Vector2 Velocity { get; }
    }
}