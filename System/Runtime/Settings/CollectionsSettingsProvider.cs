using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace VV.Collecting
{
#if UNITY_EDITOR
    public class CollectionsSettingsProvider : SettingsProvider
    {
        #if UNITY_EDITOR
        private SerializedObject _settings;
        
        public CollectionsSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            _settings = CollectionsSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // Use IMGUI to display UI:
            EditorGUILayout.PropertyField(_settings.FindProperty("activeCollections"));
            
            _settings.ApplyModifiedPropertiesWithoutUndo();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
        {
            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return new CollectionsSettingsProvider("Project/VV/Collections", SettingsScope.Project);
        }
        #endif
    }
#endif
}