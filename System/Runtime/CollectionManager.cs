using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace VV.Collecting
{
    /// <summary>
    /// TODO: Change this struct
    /// Too poor data, we need a class with access to the full data to be able to do more advanced stats
    /// </summary>
    public struct CollectionUpdateData
    {
        public int progressDelta;
        public float progressRatio;
        public int progressCount;
    }
    
    /// <summary>
    /// Static manager, used to instantiate the runtime collections.
    /// </summary>
    [Serializable]
    public static class CollectionManager
    {
        public static Dictionary<CollectionType, RuntimeCollection> RuntimeCollections { get; set; } = new();
        public static Dictionary<string, CollectionType> CollectionIdToType { get; set; } = new();

        #region Events

        public static event UnityAction RuntimeCollectionsInitialized;

        public static Dictionary<CollectionType, UnityAction<CollectionUpdateData>> OnCollectionProgressUpdated;
        public static UnityAction<CollectionType> OnCollectionInitialized;

        #endregion
        
        #region Init
        /// <summary>
        /// Called after first scene loaded and after Awake is called.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeInitialized()
        {
            try
            {
                CollectionsSettings customSettings = Resources.Load<CollectionsSettings>(CollectionsSettings.SettingsResourceFullPath);
                if(customSettings == null) return;
                
                GameObject runtimeCollections = new GameObject("Collections");
                Object.DontDestroyOnLoad(runtimeCollections);

                OnCollectionProgressUpdated =
                    new Dictionary<CollectionType, UnityAction<CollectionUpdateData>>(customSettings.activeCollections.Count);

                foreach (CollectionSO collectionSO in customSettings.activeCollections)
                {
                    string normalizedCollectionName = collectionSO.CollectionName.Replace(" ", "");
                    if (!Enum.TryParse(normalizedCollectionName, out CollectionType type))
                    {
                        Debug.LogError($"Collection {normalizedCollectionName} is not a valid collection name");
                        continue;
                    }
                    
                    GenerateCollection(collectionSO, type, runtimeCollections.transform);
                }
                
                RuntimeCollectionsInitialized?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occured while initializing Collections : {e.Message}");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Generates a RuntimeCollection game object.
        /// </summary>
        /// <param name="collectionSo"></param>
        /// <param name="collectionType"></param>
        /// <param name="parent"></param>
        private static void GenerateCollection(CollectionSO collectionSo, CollectionType collectionType, Transform parent)
        {
            try
            {
                string collectionName = $"{collectionSo.CollectionName}RuntimeCollection";

                RuntimeCollection collection;
                if (collectionSo.RuntimeCollectionTemplate)
                {
                    GameObject newRuntimeCollection = 
                        Object.Instantiate(collectionSo.RuntimeCollectionTemplate.gameObject, parent);
                    collection = newRuntimeCollection.GetComponent<RuntimeCollection>();
                }
                else
                {
                    GameObject newRuntimeCollection = new GameObject(collectionName);
                    newRuntimeCollection.transform.SetParent(parent);

                    collection = newRuntimeCollection.AddComponent<RuntimeCollection>();
                }

                collection.CollectionSO = collectionSo;
                    
                OnCollectionProgressUpdated.Add(collectionType, _ => {});
                collection.onCollectionProgressUpdate.AddListener(collectionData =>
                    OnCollectionProgressUpdated[collectionType]?.Invoke(collectionData));
                
                collection.CollectionType = collectionType;
                
                RuntimeCollections.Add(collectionType, collection);
                CollectionIdToType.Add(collectionSo.UniqueId, collectionType);
                OnCollectionInitialized?.Invoke(collectionType);
            }
            catch (Exception e)
            {
                Debug.LogError($"An error occured while initializing {collectionSo.CollectionName}'s collection : {e.Message}");
                Debug.LogException(e);
            }
        }

        public static bool IsInitialzed(string collectionId) => CollectionIdToType.ContainsKey(collectionId);
        public static bool IsInitialzed(CollectionType collectionType) =>
            CollectionIdToType.ContainsValue(collectionType);

        public static void ExecuteOnInitialized(CollectionType collectionType, UnityAction<RuntimeCollection> action)
        {
            if (IsInitialzed(collectionType))
            {
                action?.Invoke(RuntimeCollections[collectionType]);
            }
            else
            {
                OnCollectionInitialized += type =>
                {
                    if (type == collectionType) action?.Invoke(RuntimeCollections[collectionType]);
                };
            }
        }
        #endregion

        public static bool IsCollected(string collectableId)
        {
            return CollectionIdToType.TryGetValue(collectableId, out CollectionType type) && 
                   RuntimeCollections.ContainsKey(type) && 
                   RuntimeCollections[type].Contains(collectableId);
        }
        
        public static bool IsCollected(string collectionId, string collectableId)
        {
            return CollectionIdToType.TryGetValue(collectionId, out CollectionType type) && 
                   RuntimeCollections.ContainsKey(type) && 
                   RuntimeCollections[type].Contains(collectableId);
        }
        
        public static bool IsCollected(string collectableId, CollectionType type)
        {
            return RuntimeCollections.ContainsKey(type) && 
                   RuntimeCollections[type].Contains(collectableId);
        }

        public static bool IsCollected(Collectable collectable)
        {
            bool isCollected = Enum.TryParse(collectable.CollectionSO.CollectionName, out CollectionType type) &&
                               RuntimeCollections.ContainsKey(type) && 
                               RuntimeCollections[type].Contains(collectable);
            return isCollected;
        }

        public static bool CollectionActivated(CollectionType collectionType) => 
            RuntimeCollections.ContainsKey(collectionType);

        public static void QuietCollect(string collectionId, string collectableId)
        {
            if (!CollectionIdToType.TryGetValue(collectionId, out CollectionType type)) return;
            AddRuntimeCollectable(type, collectableId, true);
        }

        /// <summary>
        /// Tries to execute the collectable's behaviour.
        /// Returns true if the collectable is in the current scene and the behaviour has been executed, false elsewhere.
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="collectableId"></param>
        /// <returns></returns>
        public static bool TryExecuteBehaviour(string collectionId, string collectableId)
        {
            if (!CollectionIdToType.TryGetValue(collectionId, out CollectionType type)) return false;
            return RuntimeCollections[type].TryExecuteCollectableBehaviour(collectableId);
        }
        
        public static void ResetAllRuntimeCollectable()
        {
            foreach (CollectionType collectionType in Enum.GetValues(typeof(CollectionType)))
                ResetRuntimeCollectable(collectionType);
        }
        
        public static void ResetRuntimeCollectable(CollectionType collectionType)
        {
            if(!RuntimeCollections.TryGetValue(collectionType, out RuntimeCollection collec)) return;

            if (collec.CollectedIds.Count == 0) return;
            
            collec.CollectedIds.Clear();
        }

        /// <summary>
        /// Add collectable to the correct runtime collection.
        /// </summary>
        /// <param name="collectionType"></param>
        /// <param name="collectableId"></param>
        /// <param name="isSilent"></param>
        public static void AddRuntimeCollectable(CollectionType collectionType, string collectableId,
            bool isSilent = false)
        {
            if(string.IsNullOrEmpty(collectableId) || 
               !RuntimeCollections.TryGetValue(collectionType, out RuntimeCollection collec)) return;

            if (collec.CollectedIds.Contains(collectableId)) return;
            
            collec.CollectedIds.Add(collectableId);
            
            if(!isSilent)
                OnCollectionProgressUpdated[collectionType]?.Invoke(collec.GenerateUpdateData());
        }
        
        public static void RemoveRuntimeCollectable(CollectionType collectionType, string collectableId,
            bool isSilent = false)
        {
            if(string.IsNullOrEmpty(collectableId) || 
               !RuntimeCollections.TryGetValue(collectionType, out RuntimeCollection collec)) return;

            int index = collec.CollectedIds.FindIndex(s => s == collectableId);
            if (index == -1) return;
            
            collec.CollectedIds.RemoveAt(index);
            
            if(!isSilent)
                OnCollectionProgressUpdated[collectionType]?.Invoke(collec.GenerateUpdateData(-1));
        }

        public static int GetCollectedCurrentCount(CollectionType collectionType) => 
            CollectionActivated(collectionType) ? RuntimeCollections[collectionType].CollectedIds.Count : -1;
        public static int GetCollectionTotal(CollectionType collectionType) => 
            CollectionActivated(collectionType) ? RuntimeCollections[collectionType].CollectionSO.Count : -1;

        public static T GetRuntimeCollection<T>(CollectionType type = CollectionType.None) where T : RuntimeCollection
        {
            if (type == CollectionType.None)
            {
                return RuntimeCollections.Values.ToList().ConvertAll(x =>
                {
                    if (x is T t)
                        return t;
                    return null;
                }).FirstOrDefault();
            }

            return (T)RuntimeCollections[type];
        }
    }
}