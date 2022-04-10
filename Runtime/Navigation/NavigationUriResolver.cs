using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    //TODO: make it more generic, deep link should be able to do anything, not only navigation
    public class NavigationUriResolver
    {
        private static readonly char[] _splitPath = { '/' };
        private static readonly char[] _splitQuery = { '?', '&', '=' };

        private readonly Dictionary<string, INavigationParametersResolver> _navigationResolvers;

        public NavigationUriResolver()
        {
            _navigationResolvers = new Dictionary<string, INavigationParametersResolver>();
        }

        [MustUseReturnValue]
        public IDisposable Register([NotNull] string navigableName, [NotNull] INavigationParametersResolver resolver)
        {
            _navigationResolvers.Add(navigableName, resolver);

            return new GenericDisposable(() => { _navigationResolvers.Remove(navigableName); });
        }

        public bool FindNavigable(Uri uri, out string navigableName, out object parameters)
        {
            List<string> pathList = new List<string>();
            Dictionary<string, string> parametersList = new Dictionary<string, string>();

            string name = null;

            if (uri.LocalPath.Length > 1)
            {
                string[] pathParts = uri.LocalPath.Split(_splitPath, StringSplitOptions.RemoveEmptyEntries);

                int startingIndex = 0;
                if (pathParts.Length > 0 && pathParts[0] == "links") //TODO: make optional and configurable
                    startingIndex++;

                if (pathParts.Length > startingIndex)
                {
                    name = pathParts[startingIndex];
                    startingIndex++;
                }

                for (int i = startingIndex; i < pathParts.Length; i++)
                {
                    pathList.Add(pathParts[i]);
                }
            }

            if (uri.Query.Length > 0)
            {
                string[] queryParts = uri.Query.Split(_splitQuery, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < queryParts.Length / 2; i++)
                {
                    parametersList.Add(queryParts[2 * i], queryParts[2 * i + 1]);
                }
            }

            if (!string.IsNullOrEmpty(name) && _navigationResolvers.TryGetValue(name, out var resolver))
            {
                navigableName = name;
                parameters = resolver.FromUri(pathList, parametersList);
                return true;
            }

            navigableName = null;
            parameters = null;
            return false;
        }
    }
}