using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Variables.SaveDataVariable.Samples.Editor
{
    [CustomEditor(typeof(SaveDataExample))]
    public class SaveDataExampleEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var saveDataExample = (SaveDataExample)target;

            var inspector = new VisualElement();

            InspectorElement.FillDefaultInspector(inspector, serializedObject, this);

            var saveButton = new Button(OnSaveButtonClick) {
                text = "Save"
            };
            
            inspector.Add(saveButton);
            
            var loadButton = new Button(OnLoadButtonClick) {
                text = "Load"
            };
            
            inspector.Add(loadButton);
            
            var openSaveFileButton = new Button(OnOpenSaveFileButtonClick) {
                text = "Open Save File"
            };
            
            inspector.Add(openSaveFileButton);

            var openPersistentDataPathButton = new Button(OnOpenPersistentDataPathButtonClick) {
                text = "Open Persistent Data Path"
            };
            
            inspector.Add(openPersistentDataPathButton);

            return inspector;

            #region LocalMethods

            void OnSaveButtonClick()
            {
                saveDataExample.Save();
            }
            
            void OnLoadButtonClick()
            {
                saveDataExample.Load();
            }
            
            void OnOpenSaveFileButtonClick()
            {
                Application.OpenURL(saveDataExample.SaveFilePath);
            }
            
            void OnOpenPersistentDataPathButtonClick()
            {
                Application.OpenURL(Application.persistentDataPath);
            }

            #endregion
        }
    }
}