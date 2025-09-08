using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Variables.SaveDataVariable.Samples.Editor
{
    [CustomEditor(typeof(SavePosition))]
    public class SavePositionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var savePosition = (SavePosition)target;

            var inspector = new VisualElement();

            InspectorElement.FillDefaultInspector(inspector, serializedObject, this);

            var saveButton = new Button(savePosition.Save) {
                text = "Save"
            };
            
            inspector.Add(saveButton);
            
            var loadButton = new Button(savePosition.Load) {
                text = "Load"
            };
            
            inspector.Add(loadButton);
            
            return inspector;
        }
    }
}