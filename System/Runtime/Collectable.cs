using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VV.ID;
using VV.Utility;

namespace VV.Collecting
{
    /// <summary>
    /// Collectable in-game component.
    /// Contains the generic Collectable behaviour.
    /// </summary>
    [Serializable]
    public class Collectable : SerializedMonoBehaviour
    {
        [SerializeField] protected CollectionSO collectionSO;
        [SerializeField] [Unity.Collections.ReadOnly] protected CollectableSOBase collectableSo;

        public CollectionSO CollectionSO => collectionSO;
        public CollectableSOBase CollectableSo => collectableSo;
        
        public int Score => collectionSO.Score;
        public string CollectionName => collectionSO.CollectionName;
        
        public CollectableBaseBehaviour Behaviour => GetComponent<CollectableBaseBehaviour>();
        
        public static event Action<Collectable> CollectionSetEvent;
        public UnityEvent<Collectable> InitializedEvent = new();
        public UnityEvent<Collectable> InitializedAndAlreadyCollected = new();
        public UnityEvent<Collectable> InitializedAndNotCollected = new();
        public UnityEvent<Collectable> CollectEvent = new();

        protected virtual void Start()
        {
            // Debug.Log($"Start collectable {Id} from {CollectionName} - enabled:{enabled} - activeSelf:{gameObject.activeSelf} - activeInHierarchy:{gameObject.activeInHierarchy}");
            // Debug.Log($"Start collectable {CollectionManager.RuntimeCollections.TryGetValue()}");
            enabled = Enum.TryParse(CollectionSO.CollectionName, out CollectionType collectionType) &&
                      CollectionManager.CollectionActivated(collectionType);
            
            InitializedEvent?.Invoke(this);
            
            var isCollected = CollectionManager.IsCollected(this);
            
            if(isCollected)
                InitializedAndAlreadyCollected?.Invoke(this);
            else
                InitializedAndNotCollected?.Invoke(this);
        }

        [Button]
        public virtual void Collect()
        {
            if(!enabled) return;
            
            CollectEvent?.Invoke(this);
            CollectableSo.Collect(this);
        }
#if UNITY_EDITOR

        protected void OnCollectionSet()
        {
            CollectionSetEvent?.Invoke(this);
        }

        [Button("Generate/Retrieve Collectable Asset")]
        public virtual void GenerateCollectableSO(int index)
        {
            if(collectionSO == null) return;

            if(FindCollectableSO()) return;
            
            CollectableSOBase newCollectableSoBase = ScriptableObject.CreateInstance<CollectableSOBase>();
            newCollectableSoBase.Init(ID.ToString(), SceneManager.GetActiveScene().name);
            collectableSo = collectionSO.SaveCollectableSoAsset(newCollectableSoBase, index);
        }

        public virtual bool FindCollectableSO()
        {
            CollectableSOBase newCollectableSO = collectionSO.FindCollectableData(this);
            if (newCollectableSO == null || newCollectableSO.Assigned) return false;
            collectableSo = newCollectableSO;
            return true;
        }
#endif
    }
}