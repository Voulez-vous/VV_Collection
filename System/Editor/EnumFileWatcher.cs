using UnityEditor;

namespace VV.Collecting.Editor
{
    public class EnumFileWatcher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string deleted in deletedAssets)
            {
                if (deleted != CollectionsSettings.GetOrCreateSettings().EnumFullPath) continue;
                
                CollectionsSettings.RemoveCustomEnumDefine();
                    
                CollectionsSettings.GetOrCreateSettings().DeleteAsmRef();
            }
        }
    }
}