namespace Behc.Mvp.Models
{
    public interface IToastManager
    {
        void AddToast(object toastData);
        void ClearAllToasts();
    }
}