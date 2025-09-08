using System;
using System.Collections.Generic;
using fefek5.Variables.SaveDataVariable.Runtime;
using fefek5.Variables.SaveDataVariable.Runtime.Serializable;
using UnityEngine;

namespace fefek5.Variables.SaveDataVariable.Samples
{
    [ExecuteAlways]
    public class SaveDataExample : MonoBehaviour
    {
        #region NestedTypes
        
        [Serializable]
        public struct ExampleStruct
        {
            public string ExampleString;
            public int ExampleInt;
            public float ExampleFloat;
            public bool ExampleBool;
            public Vector3 ExampleVector3;
            public Quaternion ExampleQuaternion;
            public Color ExampleColor;
            public Vector2 ExampleVector2;
            public Vector4 ExampleVector4;
        }
        
        private enum ExampleEnum
        {
            ExampleOption1,
            ExampleOption2,
            ExampleOption3
        }
        
        [Flags]
        private enum ExampleEnumFlags
        {
            ExampleOption1 = 1 << 0,
            ExampleOption2 = 1 << 1,
            ExampleOption3 = 1 << 2,
        }

        #endregion
        
        public string SaveFilePath => $"{Application.persistentDataPath}/{_saveFilePath}";
        
        [SerializeField] private SaveData _saveData;
        
        [Tooltip("{Application.persistentDataPath}/{_saveFilePath}")]
        [SerializeField] private string _saveFilePath = "ExampleSaveData.sav";

        [SerializeField] private ExampleStruct _exampleStruct = new() {
            ExampleString = "ExampleString",
            ExampleInt = 1,
            ExampleFloat = 1.1f,
            ExampleBool = true,
            ExampleVector3 = Vector3.one,
            ExampleQuaternion = Quaternion.identity,
            ExampleColor = Color.white,
            ExampleVector2 = Vector2.one,
            ExampleVector4 = Vector4.one
        };
        [SerializeField] private ExampleEnum _exampleEnum = ExampleEnum.ExampleOption1;
        [SerializeField] private ExampleEnumFlags _exampleEnumFlags = ExampleEnumFlags.ExampleOption1;
        [SerializeField] private string _exampleString = "ExampleString";
        [SerializeField] private int _exampleInt = 1;
        [SerializeField] private float _exampleFloat = 1.1f;
        [SerializeField] private bool _exampleBool = true;
        [SerializeField] private Vector3 _exampleVector3 = Vector3.one;
        [SerializeField] private Quaternion _exampleQuaternion = Quaternion.identity;
        [SerializeField] private Color _exampleColor = Color.yellow;
        [SerializeField] private Vector2 _exampleVector2 = Vector2.one;
        [SerializeField] private Vector4 _exampleVector4 = Vector4.one;
        [SerializeField] private List<string> _exampleList = new() { "Daniela", "have", "a dog" };
        [SerializeField] private Transform _exampleTransform;

        private void Reset()
        {
            _exampleTransform = transform;
        }

        private void OnValidate()
        {
            if (!_exampleTransform) _exampleTransform = transform;
        }

        [ContextMenu("Save")]
        public void Save()
        {
            _saveData
                .SetKey("ExampleStruct", _exampleStruct)
                .SetKey("ExampleEnum", _exampleEnum)
                .SetKey("ExampleEnumFlags", _exampleEnumFlags)
                .SetKey("ExampleString", _exampleString)
                .SetKey("ExampleInt", _exampleInt)
                .SetKey("ExampleFloat", _exampleFloat)
                .SetKey("ExampleBool", _exampleBool)
                .SetKey("ExampleVector3", _exampleVector3)
                .SetKey("ExampleQuaternion", _exampleQuaternion)
                .SetKey("ExampleColor", _exampleColor)
                .SetKey("ExampleVector2", _exampleVector2)
                .SetKey("ExampleVector4", _exampleVector4)
                .SetKey("ExampleList", _exampleList)
                .SetKey("ExampleTransform", _exampleTransform);
            
            _saveData.Save(SaveFilePath);
        }

        [ContextMenu("Load")]
        public void Load()
        {
            _saveData.Load(SaveFilePath, SetValues);
        }

        private void SetValues()
        {
            _saveData
                .GetKey("ExampleStruct", new ExampleStruct(), out _exampleStruct)
                .GetKey("ExampleEnum", ExampleEnum.ExampleOption1, out _exampleEnum)
                .GetKey("ExampleEnumFlags", ExampleEnumFlags.ExampleOption1, out _exampleEnumFlags)
                .GetKey("ExampleString", string.Empty, out _exampleString)
                .GetKey("ExampleInt", 0, out _exampleInt)
                .GetKey("ExampleFloat", 0f, out _exampleFloat)
                .GetKey("ExampleBool", false, out _exampleBool)
                .GetKey("ExampleVector3", Vector3.zero, out _exampleVector3)
                .GetKey("ExampleQuaternion", Quaternion.identity, out _exampleQuaternion)
                .GetKey("ExampleColor", Color.white, out _exampleColor)
                .GetKey("ExampleVector2", Vector2.zero, out _exampleVector2)
                .GetKey("ExampleVector4", Vector4.zero, out _exampleVector4)
                .GetKey("ExampleList", new List<string>(), out _exampleList)
                .GetKey("ExampleTransform", _exampleTransform.ToSerializeTransform(), out var serializeTransform);
            
            _exampleTransform.FromSerializeTransform(serializeTransform);
        }
    }
}
