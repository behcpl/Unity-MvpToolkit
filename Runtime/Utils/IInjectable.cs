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
        void Inject(T1 p1, T2 p2, T3 p3, T4 p4);
    }

    public interface IInjectable<in T1, in T2, in T3, in T4, in T5>
    {
        void Inject(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    }

    public interface IInjectable<in T1, in T2, in T3, in T4, in T5, in T6>
    {
        void Inject(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
    }

    public interface IInjectable<in T1, in T2, in T3, in T4, in T5, in T6, in T7>
    {
        void Inject(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
    }

    public interface IInjectable<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>
    {
        void Inject(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
    }
}