using fefek5.SaveDataVariable.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.SaveDataVariable.Editor
{
    [CustomPropertyDrawer(typeof(SaveVar<>), true)]
    public class SaveVarDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new PropertyField(property);
        }
    }
}
