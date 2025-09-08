using fefek5.Common.Runtime.Helpers;
using fefek5.Variables.SaveDataVariable.Runtime.Settings;
using UnityEditor;

namespace fefek5.Variables.SaveDataVariable.Editor
{
    public static class SaveSettingsMenuItems
    {
        [MenuItem(MenuPaths.fefek5.Variables.SaveDataVariable.PATH + "/Open Default Save Settings")]
        private static void OpenDefaultSaveSettings() => EditorUtility.OpenPropertyEditor(DefaultSaveSettings.Instance);
    }
}