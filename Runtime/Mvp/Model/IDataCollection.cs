using System.Collections.Generic;

namespace Behc.Mvp.Model
{
    public interface IDataCollection
    {
        int ItemsCount { get; }
        IEnumerable<object> Items { get; }   
    }
}