using System.Collections.Generic;

namespace Behc.Mvp.Model
{
    public interface IDataCollection
    {
        IReadOnlyCollection<object> Data { get; }
    }
}