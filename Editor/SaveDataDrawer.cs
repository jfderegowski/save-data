using fefek5.Variables.SaveDataVariable.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Variables.SaveDataVariable.Editor
{
    [CustomPropertyDrawer(typeof(SaveData))]
    public class SaveDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var showButton = new Button(OnButtonShowClick) {
                text = $"Show {property.displayName}",
                style = {
                    marginRight = -2
                }
            };

            UpdateButtonState();

            showButton.TrackPropertyValue(property, _ => UpdateButtonState());

            return showButton;

            void OnButtonShowClick()
            {
                property.serializedObject.Update();
                var saveData = fieldInfo.GetValue(property.serializedObject.targetObject) as SaveData;
                if (saveData != null) 
                    SaveDataViewerWindow.ShowWindow(saveData.ToJson());
            }
            
            void UpdateButtonState()
            {
                property.serializedObject.Update();
                
                var saveData = fieldInfo.GetValue(property.serializedObject.targetObject) as SaveData;
                
                showButton.SetEnabled(saveData != null);
            }
        }
    }
}