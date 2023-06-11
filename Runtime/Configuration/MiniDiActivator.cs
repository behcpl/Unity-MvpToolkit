using System;
using System.Collections.Generic;
using System.Reflection;

namespace Behc.Configuration
{
    public static class MiniDiActivator
    {
        public static object CreateInstance(IDependencyResolver resolver, Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            if (constructors.Length != 1)
                throw new Exception($"Ambiguous call, found {constructors.Length} constructors!");

            ConstructorInfo constructorInfo = constructors[0];
            ParameterInfo[] parametersInfo = constructorInfo.GetParameters();

            if (parametersInfo.Length == 0)
            {
                return Activator.CreateInstance(type);
            }

            object[] parameters = BuildParameters(resolver, parametersInfo);
            return Activator.CreateInstance(type, parameters);
        }

        public static object CreateInstance(IDependencyResolver resolver, Type type, params object[] additionalParameters)
        {
            //TODO: resolve as usual, if parameter not found try additionalParameters in order 
            throw new NotImplementedException();
        }

        public static void CallMethod(IDependencyResolver resolver, object instance, string methodName)
        {
            MethodInfo info = FindMethodInfo(instance.GetType(), methodName);
            if (info == null)
                return;

            ParameterInfo[] parametersInfo = info.GetParameters();
            if (parametersInfo.Length == 0)
            {
                info.Invoke(instance, Array.Empty<object>());
            }

            object[] parameters = BuildParameters(resolver, parametersInfo);
            info.Invoke(instance, parameters);
        }

        private static MethodInfo FindMethodInfo(Type type, string methodName)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.Name == methodName)
                {
                    return methodInfo;
                }
            }

            return null;
        }

        private static object[] BuildParameters(IDependencyResolver resolver, IReadOnlyList<ParameterInfo> parametersInfo)
        {
            object[] parameters = new object[parametersInfo.Count];
            for (int index = 0; index < parametersInfo.Count; index++)
            {
                ParameterInfo info = parametersInfo[index];
                parameters[index] = resolver.Resolve(info.ParameterType, info.Name, true);

                if (parameters[index] == null)
                    throw new Exception($"Failed to resolve {index + 1} parameter: {info}");
            }

            return parameters;
        }
    }
}