using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VV.Utility;

namespace VV.Collecting
{
    /// <summary>
    /// Runtime Collection used to store collected objects data.
    /// </summary>
    public class RuntimeCollection : MonoBehaviour
    {
        [SerializeField] [ReadOnly]
        protected List<string> collectedIds = new();
        
        [SerializeField] private int totalScore;

        [field:SerializeField] [Utility.ReadOnly] public CollectionSO CollectionSO { get; set; }
        [field:SerializeField] [Utility.ReadOnly] public CollectionType CollectionType { get; set; }
        
        public List<string> CollectedIds => collectedIds;
        
        public static UnityAction<RuntimeCollection> AnyCollectableStored;
        public UnityEvent<RuntimeCollection> CollectableStored;
        public UnityEvent<CollectionUpdateData> onCollectionProgressUpdate = new();
        
        private void Awake()
        {
            CollectableSOBase.AnyCollected += OnCollected;
        }

        public CollectionUpdateData GenerateUpdateData(int delta = 1)
        {
            return new CollectionUpdateData
            {
                progressDelta = delta,
                progressRatio = (float)collectedIds.Count / CollectionSO.Count,
                progressCount = collectedIds.Count,
            };
        }

        private void OnCollected(Collectable collectable)
        {
            CollectableSOBase collectableSOBase = collectable.CollectableSo;
            string collectedId = collectableSOBase.UniqueId;
            if (collectedIds.Contains(collectedId) || !collectable.CollectionName.Equals(CollectionSO.CollectionName))
                return;

            Debug.Log($"Collected {collectable.CollectionName} n°{collectable.CollectableSo.UniqueId}");
            collectedIds.Add(collectedId);

            AnyCollectableStored?.Invoke(this);
            CollectableStored?.Invoke(this);
            onCollectionProgressUpdate?.Invoke(GenerateUpdateData());
            // Debug.Log($"{collectable.CollectionName} n°{collectable.CollectableSo.UniqueId} successfully collected");
        }
        
        public bool Contains(Collectable collectable) => collectedIds.Contains(collectable.CollectableSo.UniqueId);
        public bool Contains(string collectableId) => collectedIds.Contains(collectableId);
        
        /// <summary>
        /// Tries to execute the collectable's behaviour.
        /// Returns true if the collectable is in the current scene and the behaviour has been executed, false elsewhere.
        /// </summary>
        /// <param name="collectableId"></param>
        /// <returns></returns>
        public bool TryExecuteCollectableBehaviour(string collectableId)
        {
            var collectables = FindObjectsByType<Collectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Collectable collectable in collectables)
            {
                if (!collectable.CollectableSo.UniqueId.Equals(collectableId)) continue;
                collectable.Behaviour.OnCollected();
                return true;
            }
            return false;
        }
    }
}