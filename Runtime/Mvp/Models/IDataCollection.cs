using System.Collections.Generic;

namespace Behc.Mvp.Models
{
    public interface IDataCollection
    {
        IReadOnlyCollection<object> Data { get; }
    }
}