using fefek5.SaveDataVariable.Runtime;
using fefek5.Toys.Editor.Editors;
using fefek5.Toys.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace fefek5.SaveDataVariable.Editor
{
    [CustomPropertyDrawer(typeof(SaveVar), true)]
    public class SaveVarDrawer : PropertyDrawer
    {
        private SaveVar _saveVar;
        
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _saveVar = property.GetTarget<SaveVar>();

            var textField = new Label(_saveVar.StringValue);

            var saveVarProp = new FoldoutCustomHeaderElement(property.displayName, textField);

            var buttonsContent = new VisualElement() {
                style = {
                    flexDirection = FlexDirection.Row,
                }
            };
            
            var pullButton = new InspectorButtonElement(this, nameof(Pull)) {
                style = { flexGrow = 1, flexBasis = 0 }
            };
            
            var pushButton = new InspectorButtonElement(this, nameof(Push)) {
                style = { flexGrow = 1, flexBasis = 0 }
            };

            buttonsContent.Add(pullButton);
            buttonsContent.Add(pushButton);

            saveVarProp.Add(buttonsContent);
            
            return saveVarProp;
        }

        private void Pull() => _saveVar.PullAsync();

        private void Push() => _saveVar.PushAsync();
    }

    [CustomPropertyDrawer(typeof(SaveVar<>), true)]
    public class SaveVarGenericDrawer<T> : SaveVarDrawer
    {
        
    }
}
