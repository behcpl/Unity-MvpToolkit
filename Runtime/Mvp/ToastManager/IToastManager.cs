namespace Behc.Mvp.ToastManager
{
    public interface IToastManager
    {
        void AddToast(object toastData);
        void ClearAllToasts();
    }
}