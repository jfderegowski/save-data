using System;
using System.Threading;
using System.Threading.Tasks;
using fefek5.SaveDataVariable.Runtime;
using fefek5.Toys.Editor.Editors;
using fefek5.Toys.Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.SaveDataVariable.Editor
{
    [CustomPropertyDrawer(typeof(SaveVar), true)]
    public class SaveVarDrawer : PropertyDrawer
    {
        // SetValue siedzi na SaveVar<T>, a drawer widzi tylko bazę SaveVar,
        // więc InspectorButtonElement dociera do niego refleksją i sam buduje pole pod T.
        private const string SET_VALUE_METHOD = nameof(SaveVar<object>.SetValue);
        private const string RESET_TO_DEFAULT_METHOD = nameof(SaveVar<object>.ResetToDefault);

        private const int DOT_SIZE = 8;
        private const int REFRESH_INTERVAL_MS = 250;

        private static readonly Color _syncedColor = new Color32(0x4C, 0xAF, 0x50, 0xFF);
        private static readonly Color _dirtyColor = new Color32(0xE6, 0x8A, 0x00, 0xFF);
        private static readonly Color _unconfiguredColor = new Color32(0x6E, 0x6E, 0x6E, 0xFF);
        private static readonly Color _failedColor = new Color32(0xD3, 0x4B, 0x4B, 0xFF);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var saveVar = property.GetTarget<SaveVar>();

            var valueText = new TextElement() {
                text = saveVar.ToString()
            };

            var statusDot = CreateStatusDot();

            var header = new VisualElement() {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                }
            };

            header.Add(valueText);
            header.Add(statusDot);

            var foldout = new FoldoutElement(property, header);

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

            var resetButton =
                new InspectorButtonElement(saveVar, RESET_TO_DEFAULT_METHOD, "Reset To Default", property);

            var setValueButton = new InspectorButtonElement(saveVar, SET_VALUE_METHOD, "Set Value", property);

            foldout.Add(resetButton);
            foldout.Add(setValueButton);
            foldout.Add(buttonsContent);

            foldout.schedule.Execute(Refresh).Every(REFRESH_INTERVAL_MS);

            Refresh();

            return foldout;

            void Pull() => saveVar.PullAsync();

            void Push() => saveVar.PushAsync();

            void Refresh()
            {
                valueText.text = saveVar.ToString();

                var color = saveVar.IsDirty
                    ? _dirtyColor
                    : _syncedColor;

                SetDotColor(statusDot, color, true);

                statusDot.tooltip = BuildTooltip();

                // Bez ścieżki push/pull nie ma dokąd pisać, więc przyciski gasną.
                pullButton.SetEnabled(saveVar.IsDirty);
                pushButton.SetEnabled(saveVar.IsDirty);
            }

            string BuildTooltip()
            {
                var state = saveVar.IsDirty
                    ? "Not synced — changed since the last push"
                    : "Synced";

                return $"{state}\n{saveVar.RelativePath}";
            }
        }

        private static VisualElement CreateStatusDot() =>
            new() {
                style = {
                    width = DOT_SIZE,
                    height = DOT_SIZE,
                    marginLeft = 6,
                    marginRight = 2,
                    flexShrink = 0,
                    borderTopLeftRadius = DOT_SIZE,
                    borderTopRightRadius = DOT_SIZE,
                    borderBottomLeftRadius = DOT_SIZE,
                    borderBottomRightRadius = DOT_SIZE,
                    borderTopWidth = 1,
                    borderRightWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                }
            };

        private static void SetDotColor(VisualElement dot, Color color, bool filled)
        {
            dot.style.backgroundColor = filled ? color : Color.clear;

            dot.style.borderTopColor = color;
            dot.style.borderRightColor = color;
            dot.style.borderBottomColor = color;
            dot.style.borderLeftColor = color;
        }
    }
}
