using fefek5.SaveDataVariable.Runtime;
using fefek5.SaveDataVariable.Runtime.Settings;
using UnityEngine;

namespace fefek5.SaveDataVariable.Samples
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