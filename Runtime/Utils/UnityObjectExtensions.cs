using UnityEngine;

namespace Behc.Utils
{
    public static class UnityObjectExtensions
    {
        // bypass heavy unity check, with proper lifetime control of scripts, there is no need for it
        public static bool IsNull(this Object unityObject)
        {
            object obj = unityObject;
            return obj == null;
        }

        public static bool IsNotNull(this Object unityObject)
        {
            object obj = unityObject;
            return obj != null;      
        }
    }
}