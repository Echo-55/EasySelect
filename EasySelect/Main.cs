using EasySelect.Components;
using EasySelect.Utils;
using UnityEngine;
using UnityModManagerNet;
using Object = UnityEngine.Object;

// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace EasySelect
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Main
    {
        #region Private Fields

        /// <summary>
        /// Reference to the persistent game object that has the EasySelectController component.
        /// </summary>
        private static GameObject _hookObject;

        /// <summary>
        /// Reference to the EasySelectController component.
        /// </summary>
        private static EasySelectController _easySelectController;

        private static FPSDisplay _fpsDisplay;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Whether the mod is enabled.
        /// </summary>
        public static bool Enabled = true;

        /// <summary>
        /// A reference to the settings.
        /// </summary>
        public static EasySelectSettings Settings;

        /// <summary>
        /// A reference to the mod entry.
        /// </summary>
        public static UnityModManager.ModEntry ModEntry;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Method called by UMM to load the mod.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            ModEntry = modEntry;
            Settings = UnityModManager.ModSettings.Load<EasySelectSettings>(modEntry);
            modEntry.OnToggle += OnToggle;
            modEntry.OnSessionStart += OnSessionStart;
            modEntry.OnGUI += OnGUI;
            modEntry.OnSaveGUI += OnSaveGUI;
        }

        #endregion Public Methods

        #region Private Methods

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
            if (Settings.ShowFPS)
            {
                _fpsDisplay = _hookObject.AddComponent<FPSDisplay>();
                _fpsDisplay.UpdatePosition(Settings.FPSDisplayPosition);
            }

            Object.DontDestroyOnLoad(_hookObject);
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
        /// Called by UMM to draw the GUI.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        private static void OnGUI(UnityModManager.ModEntry modEntry) { Settings.Draw(modEntry); }

        /// <summary>
        /// Called by UMM when the settings are saved.
        /// </summary>
        /// <param name="modEntry">A reference to this mod.</param>
        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.OnSaveGUI();
            _easySelectController ??= EasySelectController.Instance;
            if (!_easySelectController)
            {
                ESLogger.LogError("EasySelectController not found.");
                return;
            }

            _easySelectController.UpdateKeyBinds(Settings.KeyBindSettings);

            if (Settings.ShowFPS)
            {
                _fpsDisplay ??= _hookObject.AddComponent<FPSDisplay>();
                _fpsDisplay.UpdatePosition(Settings.FPSDisplayPosition);
            }
            else
            {
                var component = _hookObject.GetComponent<FPSDisplay>();
                if (!component) return;
                Object.Destroy(component);
            }

            ESLogger.SetLogLevel(Settings.LogLevel);
        }

        #endregion Private Methods
    }
}