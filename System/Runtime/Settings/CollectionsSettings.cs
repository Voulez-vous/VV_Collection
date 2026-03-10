using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VV.Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VV.Collecting
{
    public class CollectionsSettings : ScriptableObject
    {
        public static string SettingsName => "CollectionsSettings";
        public static string SettingsResourcePath => $"VV/Collecting/";
        public static string SettingsResourceFullPath => $"{SettingsResourcePath}{SettingsName}";
        public static string SettingsPath => $"Assets/Resources/{SettingsResourcePath}";
        public static string SettingsFullPath => $"{SettingsPath}/{SettingsName}.asset";
        
        [SerializeField] public List<CollectionSO> activeCollections = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            GenerateEnum();
        }

        [Button]
        public void GenerateEnum()
        {
            string fileName = "CollectionType.cs";
            string namespaceEnum = "Collecting";
            string assetPath = Path.GetDirectoryName("Packages/com.vv.collection/System/");
            string path = string.Concat(assetPath, Path.DirectorySeparatorChar, fileName);
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            fs.SetLength(0);
            StreamWriter sr = new StreamWriter(fs);
            sr.Write("using System;\n" +
                     $"namespace VV.{namespaceEnum}\n" +
                     "{\n" +
                     "    [Serializable]\n" +
                     $"    public enum {fileName.Replace(".cs", "")}\n" +
                     "    {\n");
            foreach (CollectionSO collection in 
                     activeCollections
                         .Where(collection => 
                             collection != null && !string.IsNullOrEmpty(collection.CollectionName)))
            {
                sr.WriteLine($"        {collection.CollectionName.Replace(" ", "")},");
            }
            sr.WriteLine("        None,");
            
            sr.Write("    }\n" +
                     "}");
            sr.Close();

            EditorApplication.delayCall += RefreshAssetDelayed;
        }

        protected virtual void RefreshAssetDelayed()
        {
            AssetDatabase.Refresh();
            EditorApplication.delayCall -= RefreshAssetDelayed;
        }
        
        public void FindCollections()
        {
            activeCollections.Clear();

            string[] guids = AssetDatabase.FindAssets("t:CollectionSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CollectionSO collection = AssetDatabase.LoadAssetAtPath<CollectionSO>(path);

                if (collection != null && !activeCollections.Contains(collection))
                {
                    activeCollections.Add(collection);
                }
            }

            // Save the updated asset
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CollectionsSettings] Found and assigned {activeCollections.Count} collections.");
        }
        
        [ContextMenu("Find All Collections")]
        private void FindAllCollectionsFromContextMenu()
        {
            FindCollections();
        }

        private static CollectionsSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CollectionsSettings>(SettingsFullPath);
            
            if (settings != null) return settings;
            
            if(!Directory.Exists(SettingsPath))
                Directory.CreateDirectory(SettingsPath);
            
            settings = CreateInstance<CollectionsSettings>();
            settings.FindCollections();
            AssetDatabase.CreateAsset(settings, SettingsFullPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
#endif
    }
}