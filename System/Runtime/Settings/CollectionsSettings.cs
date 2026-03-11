using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VV.Utility;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif

namespace VV.Collecting
{
    public sealed class CollectionsSettings : ScriptableObject
    {
        public static string SettingsName => "CollectionsSettings";
        public static string SettingsResourcePath => $"VV/Collecting/";
        public static string SettingsResourceFullPath => $"{SettingsResourcePath}{SettingsName}";
        public static string SettingsPath => $"Assets/Resources/{SettingsResourcePath}";
        public static string SettingsFullPath => $"{SettingsPath}/{SettingsName}.asset";
        
        public static readonly string EnumFileName = "CollectionType.cs";
        
        public static string AsmRefName => "voulezvous.collection.enum.asmref";
        
        [SerializeField] public List<CollectionSO> activeCollections = new();
        public string enumFolderPath = "Assets/Resources/Collecting/";
        
        public string EnumFullPath => $"{enumFolderPath}{EnumFileName}";
        public string AsmRefFullPath => $"{enumFolderPath}{AsmRefName}";

#if UNITY_EDITOR
        private void OnValidate()
        {
            // GenerateEnum();
        }

        [Button(engine = AttributeEngine.UIToolkit)]
        public void GenerateEnum()
        {
            string namespaceEnum = "Collecting";
            string folderPath = "Assets/Resources/Collecting/";
            string assetPath = Path.GetDirectoryName(folderPath);
            
            if(!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            string path = string.Concat(assetPath, Path.DirectorySeparatorChar, EnumFileName);
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
            fs.SetLength(0);
            StreamWriter sr = new StreamWriter(fs);
            sr.Write("using System;\n" +
                     $"namespace VV.{namespaceEnum}\n" +
                     "{\n" +
                     "    [Serializable]\n" +
                     $"    public enum {EnumFileName.Replace(".cs", "")}\n" +
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

            CreateAsmRef();

            CreateCustomEnumDefine();

            EditorApplication.delayCall += RefreshAssetDelayed;
        }

        public static void CreateCustomEnumDefine()
        {
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out string[] defines);
            
            List<string> definesList = defines.ToList();

            if(!definesList.Contains(CollectionConstants.CollectionTypeDefineName))
                definesList.AddUnique(CollectionConstants.CollectionTypeDefineName);
            
            defines = definesList.ToArray();
            
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }
        
        public static void RemoveCustomEnumDefine()
        {
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );

            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out string[] defines);

            ArrayUtility.Remove(ref defines, CollectionConstants.CollectionTypeDefineName);

            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }

        /// <summary>
        /// Allows the package access to the enum generated in the project.
        /// </summary>
        public void CreateAsmRef()
        {
            if(!File.Exists(AsmRefFullPath))
                File.WriteAllText(AsmRefFullPath, "{\n    \"reference\": \"CollectionSystemAssembly\"\n}");
        }

        public void DeleteAsmRef()
        {
            if(File.Exists(AsmRefFullPath))
                File.Delete(AsmRefFullPath);
        }

        private void RefreshAssetDelayed()
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

        public static CollectionsSettings GetOrCreateSettings()
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