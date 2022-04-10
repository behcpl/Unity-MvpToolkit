using UnityEngine;

namespace Behc.Utils
{
    // [CreateAssetMenu(fileName = "AbstractProvider", menuName = "Game/Service/Provider", order = 0)]
    public abstract class AbstractProvider<TService> : ScriptableObject
    {
        public abstract TService GetInstance();
    }
}