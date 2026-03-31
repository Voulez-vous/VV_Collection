using UnityEngine;

namespace VV.Collecting
{
    public abstract class CollectionBehaviour : ScriptableObject
    {
        public virtual void Init(RuntimeCollection collection) { }
    }
}