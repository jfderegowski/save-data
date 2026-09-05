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

            Exception lastError = null;

            var valueText = new TextElement() {
                text = saveVar.StringValue
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

            saveVar.onSyncFailed += OnSyncFailed;
            saveVar.onIsSyncChanged += OnIsSyncChanged;

            foldout.RegisterCallback<DetachFromPanelEvent>(_ => {
                saveVar.onSyncFailed -= OnSyncFailed;
                saveVar.onIsSyncChanged -= OnIsSyncChanged;
            });

            foldout.schedule.Execute(Refresh).Every(REFRESH_INTERVAL_MS);

            Refresh();

            return foldout;

            void Pull()
            {
                lastError = null;
                LogFailure(saveVar.PullAsync());
            }

            void Push()
            {
                lastError = null;
                LogFailure(saveVar.PushAsync());
            }

            void OnSyncFailed(Exception exception)
            {
                lastError = exception;
                Refresh();
            }

            void OnIsSyncChanged(bool isSync)
            {
                if (isSync) lastError = null;
            }

            void Refresh()
            {
                valueText.text = saveVar.StringValue;

                var color = lastError != null ? _failedColor
                    : !saveVar.IsConfigured ? _unconfiguredColor
                    : saveVar.IsSync ? _syncedColor
                    : _dirtyColor;

                // Puste kółko = plik nie został jeszcze wczytany, więc zmienna raportuje default.
                var filled = lastError != null || !saveVar.IsConfigured || saveVar.IsLoaded;

                SetDotColor(statusDot, color, filled);

                statusDot.tooltip = BuildTooltip();

                // Bez ścieżki push/pull nie ma dokąd pisać, więc przyciski gasną.
                pullButton.SetEnabled(saveVar.IsConfigured);
                pushButton.SetEnabled(saveVar.IsConfigured);
            }

            string BuildTooltip()
            {
                if (lastError != null)
                    return $"Last sync failed — {lastError.Message}";

                if (!saveVar.IsConfigured)
                    return $"No {nameof(SaveVar.RelativePath)} — the value never reaches the disk";

                var state = saveVar.IsSync
                    ? "Synced"
                    : "Not synced — changed since the last push";

                var loaded = saveVar.IsLoaded
                    ? string.Empty
                    : "\nFile not loaded — reporting the default value";

                return $"{state}\n{saveVar.RelativePath}{loaded}";
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

        // Push/Pull odpalamy bez await, więc wyjątek wylądowałby w nieobserwowanym
        // Tasku i zniknął. Odczyt Exception oznacza go jako obsłużony.
        private static void LogFailure(Task task) =>
            task.ContinueWith(
                faulted => Debug.LogException(faulted.Exception.GetBaseException()),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);

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
