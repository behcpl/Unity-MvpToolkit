using UnityEngine;

namespace Behc.Mvp.ToastManager
{
    public interface IToastPresenterOptions
    {
        float LifeTime { get; }
        Vector2 Velocity { get; }
    }
}