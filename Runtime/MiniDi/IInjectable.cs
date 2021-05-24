namespace Behc.MiniDi
{
    public interface IInjectable<T1>
    {
        void Inject(in T1 p1);
    }

    public interface IInjectable<T1, T2>
    {
        void Inject(in T1 p1, in T2 p2);
    }

    public interface IInjectable<T1, T2, T3>
    {
        void Inject(in T1 p1, in T2 p2, in T3 p3);
    }

    public interface IInjectable<T1, T2, T3, T4>
    {
        void Inject(in T1 p1, in T2 p2, in T3 p3, in T4 p4);
    }
}