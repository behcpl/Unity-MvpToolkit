using Behc.Mvp;

namespace Behc.Configuration
{
    public interface ISceneConfigurator<in TContext>
    {
        void Load(TContext ctx);
        void Unload(TContext ctx);
    }
}