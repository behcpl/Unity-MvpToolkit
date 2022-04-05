namespace Behc.Utils
{
    public interface IFactory<out TInstance>
    {
        TInstance Create();
    }

    public interface IFactory<out TInstance, in TParam1>
    {
        TInstance Create(TParam1 param1);
    }

    public interface IFactory<out TInstance, in TParam1, in TParam2>
    {
        TInstance Create(TParam1 param1, TParam2 param2);
    }

    public interface IFactory<out TInstance, in TParam1, in TParam2, in TParam3>
    {
        TInstance Create(TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public interface IFactory<out TInstance, in TParam1, in TParam2, in TParam3, in TParam4>
    {
        TInstance Create(TParam1 param1, TParam2 param2, TParam3 param3, TParam4 param4);
    }
}