using System.Collections.Generic;

namespace Behc.Navigation
{
    public interface INavigationParametersResolver
    {
        object FromUri(IReadOnlyList<string> uriPath, IReadOnlyDictionary<string, string> uriParameters);
    }
}