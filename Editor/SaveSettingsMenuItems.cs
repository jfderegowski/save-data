using fefek5.SaveDataVariable.Runtime.Settings;
using UnityEditor;

namespace fefek5.SaveDataVariable.Editor
{
    public static class SaveSettingsMenuItems
    {
        [MenuItem("Window/Open Default Save Settings")]
        private static void OpenDefaultSaveSettings() => EditorUtility.OpenPropertyEditor(DefaultSaveSettings.Instance);
    }
}