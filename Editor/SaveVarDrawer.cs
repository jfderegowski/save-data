using fefek5.SaveDataVariable.Runtime;
using fefek5.Toys.Editor.Editors;
using fefek5.Toys.Editor.VisualElements;
using UnityEditor;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace fefek5.SaveDataVariable.Editor
{
    [CustomPropertyDrawer(typeof(SaveVar), true)]
    public class SaveVarDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var saveVar = property.GetTarget<SaveVar>();

            var valueText = new TextElement() {
                text = saveVar.StringValue
            };
            
            var foldout = new FoldoutElement(property, valueText);

            var buttonsContent = new VisualElement() {
                style = {
                    flexDirection = FlexDirection.Row,
                }
            };

            var pullButton = new InspectorButtonElement(Pull, "Pull") {
                style = { flexGrow = 1, flexBasis = 0 },
            };

            var pushButton = new InspectorButtonElement(Push, "Push") {
                style = { flexGrow = 1, flexBasis = 0 },
            };

            buttonsContent.Add(pullButton);
            buttonsContent.Add(pushButton);

            foldout.Add(buttonsContent);
            
            return foldout;
            
            void Pull() => saveVar.PullAsync();

            void Push() => saveVar.PushAsync();
        }
    }
}
