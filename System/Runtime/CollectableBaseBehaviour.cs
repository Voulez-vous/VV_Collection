using System;
using UnityEngine;
using UnityEngine.Events;

namespace VV.Collecting
{
    [RequireComponent(typeof(Collectable))]
    public class CollectableBaseBehaviour : MonoBehaviour
    {
        public UnityEvent BehaviourExecuted = new();
        
        protected virtual void Start() {
            var collectable = GetComponent<Collectable>();
            // Debug.Log($"Behaviour of {collectable.CollectionName} n°{collectable.Id} - enabled:{enabled} - activeSelf:{gameObject.activeSelf} - activeInHierarchy:{gameObject.activeInHierarchy}");
            enabled = Enum.TryParse(collectable.CollectionSO.CollectionName, out CollectionType collectionType) &&
                      CollectionManager.CollectionActivated(collectionType);
        }

        public virtual void OnCollected()
        {
            BehaviourExecuted?.Invoke();
        }

        public virtual void OnRollback()
        {
            var collectable = GetComponent<Collectable>();
            CollectionManager.RemoveRuntimeCollectable(Enum.Parse<CollectionType>(collectable.CollectionName),
                collectable.CollectableSo.UniqueId);
        }
    }
}