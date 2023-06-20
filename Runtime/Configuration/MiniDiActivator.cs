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

            object[] parameters = BuildParameters(resolver, parametersInfo, Array.Empty<object>());
            return Activator.CreateInstance(type, parameters);
        }

        public static object CreateInstance(IDependencyResolver resolver, Type type, params object[] additionalParameters)
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

            object[] parameters = BuildParameters(resolver, parametersInfo, additionalParameters);
            return Activator.CreateInstance(type, parameters);
        }

        public static object CallMethod(IDependencyResolver resolver, object instance, string methodName)
        {
            MethodInfo info = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (info == null)
                return null;

            ParameterInfo[] parametersInfo = info.GetParameters();
            if (parametersInfo.Length == 0)
            {
                info.Invoke(instance, Array.Empty<object>());
            }

            object[] parameters = BuildParameters(resolver, parametersInfo, Array.Empty<object>());
            return info.Invoke(instance, parameters);
        }
        
        public static object CallMethod(IDependencyResolver resolver, object instance, string methodName, params object[] additionalParameters)
        {
            MethodInfo info = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (info == null)
                return null;

            ParameterInfo[] parametersInfo = info.GetParameters();
            if (parametersInfo.Length == 0)
            {
                info.Invoke(instance, Array.Empty<object>());
            }

            object[] parameters = BuildParameters(resolver, parametersInfo, additionalParameters);
            return info.Invoke(instance, parameters);
        }
        
        public static object CallMethod(IDependencyResolver resolver, Type type, string methodName)
        {
            MethodInfo info = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (info == null)
                return null;

            ParameterInfo[] parametersInfo = info.GetParameters();
            if (parametersInfo.Length == 0)
            {
                info.Invoke(null, Array.Empty<object>());
            }

            object[] parameters = BuildParameters(resolver, parametersInfo, Array.Empty<object>());
            return info.Invoke(null, parameters);
        }
        
        public static object CallMethod(IDependencyResolver resolver, Type type, string methodName, params object[] additionalParameters)
        {
            MethodInfo info = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            if (info == null)
                return null;

            ParameterInfo[] parametersInfo = info.GetParameters();
            if (parametersInfo.Length == 0)
            {
                info.Invoke(null, Array.Empty<object>());
            }

            object[] parameters = BuildParameters(resolver, parametersInfo, additionalParameters);
            return info.Invoke(null, parameters);
        }
        
        private static object[] BuildParameters(IDependencyResolver resolver, IReadOnlyList<ParameterInfo> parametersInfo, IReadOnlyList<object> additionalParameters)
        {
            int consumedAdditionalParameters = 0;
            object[] parameters = new object[parametersInfo.Count];
            for (int index = 0; index < parametersInfo.Count; index++)
            {
                ParameterInfo info = parametersInfo[index];
                parameters[index] = resolver.Resolve(info.ParameterType, info.Name, true);

                if (parameters[index] != null)
                    continue;
                
                if (consumedAdditionalParameters < additionalParameters.Count)
                {
                    object ap = additionalParameters[consumedAdditionalParameters];
                    if (info.ParameterType.IsInstanceOfType(ap))
                    {
                        parameters[index] = ap;
                        consumedAdditionalParameters++;
                        continue;
                    }
                }
                    
                throw new Exception($"Failed to resolve {index + 1} parameter: {info}");
            }

            return parameters;
        }
    }
}