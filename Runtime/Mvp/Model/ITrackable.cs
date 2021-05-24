namespace Behc.Mvp.Model
{
    public interface ITrackable
    {
        void Acquire();
        void Release();
    }
}