using EasySelect.Utils;
using UnityEngine;
using UnityModManagerNet;

// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace EasySelect
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Main
    {
        /// <summary>
        /// Reference to the persistent game object that has the EasySelectController component.
        /// </summary>
        private static GameObject _hookObject;

        /// <summary>
        /// Reference to the EasySelectController component.
        /// </summary>
        private static EasySelectController _easySelectController;

        /// <summary>
        /// Whether the mod is enabled.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// A reference to the settings.
        /// </summary>
        public static Settings Settings;

        /// <summary>
        /// A reference to the mod entry.
        /// </summary>
        public static UnityModManager.ModEntry ModEntry;

        /// <summary>
        /// Method called by UMM to load the mod.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            modEntry.OnToggle += OnToggle;
            modEntry.OnSessionStart += OnSessionStart;
            modEntry.OnGUI += OnGUI;
            modEntry.OnSaveGUI += OnSaveGUI;
        }

        /// <summary>
        /// Called by UMM when the mod is toggled.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        /// <param name="enabled">The new state.</param>
        /// <returns></returns>
        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool enabled)
        {
            Enabled = enabled;

            if (Enabled)
            {
                OnSessionStart(modEntry);
            }
            else
            {
                Object.Destroy(_hookObject);
                _hookObject = null;
            }

            return true;
        }

        /// <summary>
        /// Called by UMM when the session starts.
        /// Not really sure how they decide when to call this.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        private static void OnSessionStart(UnityModManager.ModEntry modEntry)
        {
            if (_hookObject) return;

            _hookObject = new GameObject("EasySelect");
            _easySelectController = _hookObject.AddComponent<EasySelectController>();
            Object.DontDestroyOnLoad(_hookObject);
        }

        /// <summary>
        /// Called by UMM to draw the GUI.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Draw(modEntry);
        }

        /// <summary>
        /// Called by UMM when the settings are saved.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.OnSaveGUI();
            _easySelectController ??= EasySelectController.Instance;
            if (!_easySelectController) return;
            _easySelectController.UpdateKeyBinds(Settings.KeyBinds);
            ESLogger.SetDebugMode(Settings.DebugMode);
        }
    }

    /// <summary>
    /// A class to hold the EasySelect key binds.
    /// </summary>
    [DrawFields(DrawFieldMask.Public)]
    public class KeyBinds
    {
        public KeyCode SelectNextLoco = KeyCode.RightArrow;
        public KeyCode SelectPreviousLoco = KeyCode.LeftArrow;
        public KeyCode HaltTheCurrentLoco = KeyCode.Backspace;
        public KeyCode ReleaseAllHandBrakes = KeyCode.Period;
        public KeyCode ConnectAllGladhands = KeyCode.Comma;
        public KeyCode FollowSelectedTrain = KeyCode.V;
        public KeyCode JumpToLastCarDestination = KeyCode.Slash; // Forward slash = /, Backward slash = \
    }

    /// <summary>
    /// A class to hold the EasySelect mouse settings.
    /// </summary>
    [DrawFields(DrawFieldMask.Public)]
    public class MouseSettings
    {
        public bool DisableDoubleClickFollow = false;
        public float DoubleClickTime = 0.3f;
    }

    /// <summary>
    /// Class to hold EasySelect mod settings.
    /// </summary>
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Keybinds", DrawType.Auto, Collapsible = true)]
        public KeyBinds KeyBinds = new();

        [Draw("Click Settings", DrawType.Auto, Collapsible = true)]
        public MouseSettings MouseSettings = new();

        [Draw("Debug Mode", Tooltip = "Will print more info to the console.")]
        public bool DebugMode = false;

        public void OnSaveGUI()
        {
            Save(Main.ModEntry);
        }

        /// <summary>
        /// Called when values change.
        /// Beware, this is called a lot when updating sliders.
        /// </summary>
        public void OnChange()
        {
        }
    }
}