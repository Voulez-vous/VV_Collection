using System;
using UnityEngine;
using UnityEngine.Events;
using VV.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VV.Collecting
{
    /// <summary>
    /// Base class for collectable items.
    /// A collectable is not necessarily a GameObject.
    /// </summary>
    [Serializable]
    public class CollectableSOBase : ScriptableObject
    {
        public event UnityAction<Collectable> Collected;
        public static event UnityAction<Collectable> AnyCollected;
        public static event UnityAction<CollectableSOBase> CollectableCreated;
        public static event UnityAction<CollectableSOBase> CollectableDestroyed;
        
        [SerializeField] [ReadOnly] protected string uniqueId;
        [SerializeField] [ReadOnly] protected string instanceId;
        
        public string UniqueId => uniqueId;
        public string InstanceId => instanceId;
        
        [SerializeField] [ReadOnly] protected bool assigned;
        [SerializeField] [ReadOnly] protected string sceneName;

        public bool Assigned
        {
            get => assigned;
            set => assigned = value;
        }
        
        public string SceneName
        {
            get => sceneName;
            set => sceneName = value;
        }

        public void Init(string newInstanceId, string newSceneName)
        {
            instanceId = newInstanceId;
            sceneName = newSceneName;
            assigned = true;
        }

        public virtual void Collect(Collectable collected)
        {
            Collected?.Invoke(collected);
            AnyCollected?.Invoke(collected);
        }
        
        public bool Equals(CollectableSOBase x, CollectableSOBase y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(CollectableSOBase obj)
        {
            return HashCode.Combine(obj.name, obj.UniqueId);
        }
        
#if UNITY_EDITOR
        [Button]
        public virtual void GenerateNewGuid()
        {
            uniqueId = Guid.NewGuid().ToString();
            Save();
            Debug.Log($"{name} unique id updated to {uniqueId} with Button");
        }
        
        protected virtual void Awake()
        {
            if (!String.IsNullOrEmpty(UniqueId)) return;
            
            Debug.Log($"{name} unique id {uniqueId}");
            GenerateNewGuid();
            Debug.Log($"{name} unique id updated to {uniqueId} with Awake");
            
            CollectableCreated?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            CollectableDestroyed?.Invoke(this);
        }
        
        #region Reliable In-Editor OnDestroy

        // Sadly OnDestroy is not being called reliably by the editor. So we need this.
        // Thanks to: https://discussions.unity.com/t/845563/6
        class OnDestroyProcessor : AssetModificationProcessor
        {
            // Cache the type for reuse.
            static readonly Type _type = typeof(CollectableSOBase);

            // Limit to certain file endings only.
            static readonly string _fileEnding = ".asset";

            public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions _)
            {
                if (!path.EndsWith(_fileEnding))
                    return AssetDeleteResult.DidNotDelete;

                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType != null && (assetType == _type || assetType.IsSubclassOf(_type)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<CollectableSOBase>(path);
                    asset.OnDestroy();
                }

                return AssetDeleteResult.DidNotDelete;
            }
        }

        #endregion

        protected virtual void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}