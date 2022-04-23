namespace Behc.Mvp.Models
{
    public interface ITrackable
    {
        void Acquire();
        void Release();
    }
}