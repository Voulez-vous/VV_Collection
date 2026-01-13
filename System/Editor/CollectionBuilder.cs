using System;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace VV.Collecting.Editor
{
    [CustomEditor(typeof(CollectionSO))]
    [Serializable]
    public class CollectionBuilder : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var script = (CollectionSO)serializedObject.targetObject;

            // Add the default inspector without triggering recursion
            SerializedProperty iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true)) // Skip m_Script
            {
                do
                {
                    if (iterator.name == "m_Script") continue; // Skip the script reference

                    var propertyCopy = serializedObject.FindProperty(iterator.propertyPath);
                    var field = new PropertyField(propertyCopy);
                    field.Bind(serializedObject); // Safe binding
                    root.Add(field);

                } while (iterator.NextVisible(false));
            }

            root.Add(new VisualElement { style = { height = 10 } });

            // Add buttons
            root.Add(new Button(() => script.GenerateCollectables()) { text = "Generate Collectables" });
            root.Add(new Button(() => script.GenerateNewGUID()) { text = "Generate GUID" });
            root.Add(new Button(() => script.ResaveAllCollectables()) { text = "Update Collectables" });

            return root;
        }
    }
}
