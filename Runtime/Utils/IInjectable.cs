namespace Behc.Mvp.Presenter
{
    public interface IInjectable<in T1>
    {
        void Inject(T1 p1);
    }

    public interface IInjectable<in T1, in T2>
    {
        void Inject(T1 p1, T2 p2);
    }

    public interface IInjectable<in T1, in T2, in T3>
    {
        void Inject(T1 p1, T2 p2, T3 p3);
    }

    public interface IInjectable<in T1, in T2, in T3, in T4>
    {
        void Inject(T1 p1, T2 p2, T3 p3,  T4 p4);
    }
}