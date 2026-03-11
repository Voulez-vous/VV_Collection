using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VV.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace VV.Collecting
{
    /// <summary>
    /// ScriptableObject collection, contains all CollectableSO references.
    /// Auto referenced.
    /// </summary>
    [CreateAssetMenu(menuName = "VV/Collectables/CollectionSO", fileName = "CollectionSO")]
    public class CollectionSO : ScriptableObject
    {
        [SerializeField] [ReadOnly] private string uniqueId;
        
        public string UniqueId => uniqueId;
        
        [SerializeField] private List<CollectableSOBase> collectableCollection;
        [SerializeField] private string collectionName;
        [SerializeField] private int score;
        [SerializeField] private RuntimeCollection runtimeCollectionTemplate;

        public string CollectionName => collectionName;
        
        public int Score => score;
        
        public int Count => collectableCollection.Count;
        
        public RuntimeCollection RuntimeCollectionTemplate => runtimeCollectionTemplate;

        protected virtual void OnEnable()
        {
            CollectableSOBase.CollectableCreated += OnCollectableCreated;
            CollectableSOBase.CollectableDestroyed += OnCollectableDestroyed;
        }

        protected virtual void OnDisable()
        {
            CollectableSOBase.CollectableCreated -= OnCollectableCreated;
            CollectableSOBase.CollectableDestroyed -= OnCollectableDestroyed;
        }

        protected virtual void OnCollectableCreated(CollectableSOBase collectable)
        {
            if (!collectable.name.Contains(collectionName) || collectableCollection.Contains(collectable)) return;
            
            collectableCollection.Add(collectable);
        }
        
        protected virtual void OnCollectableDestroyed(CollectableSOBase collectable)
        {
            if(!collectableCollection.Contains(collectable))
                return;
            
            collectableCollection.Remove(collectable);
        }

        public virtual void Add(CollectableSOBase collectable)
        {
            if(collectableCollection.Contains(collectable))
                return;
            
            collectableCollection.Add(collectable);
        }

#if UNITY_EDITOR
        [Header("Settings")]
        public string CurrentFolder {
            get
            {
                string[] folders = AssetDatabase.GetAssetPath(this).Split('/');
                folders = folders.Take(folders.Length - 1).ToArray();
                return String.Join('/', folders);
            }
        }
        
        public string FolderCollectionPath => Path.Combine(CurrentFolder, collectionName);

        public CollectableSOBase SaveCollectableSoAsset(CollectableSOBase newCollectableSoBase, int index)
        {
            if (String.IsNullOrEmpty(collectionName))
            {
                Debug.LogError($"Collection {collectionName} has no name !");
                return null;
            }
            Debug.Log($"{FolderCollectionPath}");
            
            if(!AssetDatabase.IsValidFolder(FolderCollectionPath))
                AssetDatabase.CreateFolder(CurrentFolder, collectionName);

            AssetDatabase.CreateAsset(newCollectableSoBase, Path.Combine(FolderCollectionPath, $"{collectionName} {index}.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newCollectableSoBase;

            Add(newCollectableSoBase);

            return newCollectableSoBase;
        }

        public CollectableSOBase FindCollectableData(Collectable target)
        {
            string[] guids = AssetDatabase.FindAssets($"t:CollectableSOBase {target.name}");
            
            if(guids.Length == 0) return null;

            var guid = guids.FirstOrDefault();
            
            if(guid == null) return null;
            
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<CollectableSOBase>(path);
        }
        
        /// <summary>
        /// Iterates through every scene to retrieve every collectable associated to this collection and create a CollectableSOBase.
        /// </summary>
        public void GenerateCollectables()
        {
            if (String.IsNullOrEmpty(collectionName))
            {
                Debug.LogError($"Collection {collectionName} has no name !");
                return;
            }
            
            int i = 1;
            string currentScenePath = SceneManager.GetActiveScene().path;
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            Debug.Log($"{guids.Length} to explore :");
            foreach (string guid in guids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Processing scene: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                
                Debug.Log($"Searching in {scene.path}...");
                bool modified = false;
                foreach (Collectable collectable in FindObjectsByType<Collectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if(!collectable || !collectable.CollectionSO.Equals(this)) continue;
                    Debug.Log($"Converting {collectable.name}...");
                    
                    collectable.GenerateCollectableSO(i);
                    collectable.gameObject.name = $"{collectable.CollectableSo.name}";
                    
                    i++;
                    EditorUtility.SetDirty(collectable.gameObject);
                    EditorUtility.SetDirty(collectable);
                    modified = true;
                }

                if (modified)
                {
                    EditorSceneManager.MarkSceneDirty(scene); // Mark scene dirty
                    EditorSceneManager.SaveScene(scene);      // Save scene
                    Debug.Log($"Saved changes to scene: {scenePath}");
                }
            }
            
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);

            Save();
        }

        /// <summary>
        /// Used to fix a serialisation issue.
        /// </summary>
        public virtual void ResaveAllCollectables()
        {
            var assetGUIDs = AssetDatabase.FindAssets("t:CollectableSOBase", new[] { FolderCollectionPath });

            foreach (var assetGUID in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                CollectableSOBase collectableSO = AssetDatabase.LoadAssetAtPath<CollectableSOBase>(assetPath);
                
                collectableSO.GenerateNewGuid();
                
                if(!collectableCollection.Contains(collectableSO)) 
                    collectableCollection.Add(collectableSO);

                Save();
            }
        }
        
        public virtual void GenerateNewGuid()
        {
            if(!String.IsNullOrEmpty(uniqueId)) return;
            
            uniqueId = Guid.NewGuid().ToString();
            Save();
            Debug.Log($"New {name} unique id : {uniqueId}");
        }

        protected virtual void Awake()
        {
            GenerateNewGuid();
        }

        protected virtual void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}