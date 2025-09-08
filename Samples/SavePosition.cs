using fefek5.Variables.SaveDataVariable.Runtime;
using fefek5.Variables.SaveDataVariable.Runtime.Settings;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Samples
{
    public class SavePosition : MonoBehaviour
    {
        [SerializeField] private SaveData _saveData;
        [Tooltip("{Application.persistentDataPath}/{_saveFilePath}")]
        [SerializeField] private string _saveFilePath = "ExampleSaveData.sav";
        [SerializeField] private SaveSettings _saveSettings = new();
        
        public void Save()
        {
            
        }
        
        public void Load()
        {
            
        }
    }
}