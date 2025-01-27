using EasySelect.Utils;
using HarmonyLib;
using Helpers;
using JetBrains.Annotations;
using Model;
using UnityEngine;
using Input = UnityEngine.Input;

namespace EasySelect
{
    public class EasySelectController : MonoBehaviour
    {
        #region Private Fields

        [CanBeNull] private static Harmony _harmony;
        [CanBeNull] private KeyBinds _keyBinds;
        [CanBeNull] private KeyInputHandler _keyInputHandler;

        [CanBeNull] private Camera _mainCamera;

        // asset bundle references
        private const string AssetBundlePath = "/EasySelectIcons";
        [CanBeNull] private static AssetBundle _assetBundle;
        [CanBeNull] private static Sprite _uncoupleSprite;
        [CanBeNull] private static Sprite _coupleSprite;

        // double click detection
        private float _lastClickTime;
        // private const float DoubleClickThreshold = 0.3f;

        private readonly int _pickableLayerMask = (1 << ObjectPicker.LayerClickable) | (1 << Layers.UI) |
                                                  (1 << Layers.Default) |
                                                  (1 << Layers.Terrain);

        #endregion Private Fields

        #region Public Properties

        [CanBeNull] public static Sprite UncoupleSprite => _uncoupleSprite;
        [CanBeNull] public static Sprite CoupleSprite => _coupleSprite;
        [CanBeNull] public static EasySelectController Instance { get; private set; }

        #endregion Public Properties

        #region Unity Methods

        private void Awake()
        {
            // set the singleton instance
            if (!Instance)
            {
                Instance = this;
            }
            else // if there is already an instance, destroy this new one and only keep the one that was created first
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            ESLogger.LogDebug("EasySelect enabled.");

            _harmony ??= new Harmony("EasySelect");
            _harmony.PatchAll();

            _keyBinds ??= Main.Settings.KeyBinds;
            _keyInputHandler ??= new KeyInputHandler(_keyBinds);
        }

        private void OnDisable()
        {
            ESLogger.LogDebug("EasySelect disabled.");
            _harmony?.UnpatchAll();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _harmony?.UnpatchAll();
        }

        private void Start()
        {
            ESLogger.Log("EasySelect started.");

            _keyBinds ??= Main.Settings.KeyBinds;
            _keyInputHandler ??= new KeyInputHandler(_keyBinds);

            _assetBundle ??= AssetBundle.LoadFromFile($"{Main.ModEntry.Path}{AssetBundlePath}");
            UtilityClass.TryLoadSprites(_assetBundle, out _coupleSprite, out _uncoupleSprite);
        }

        private void Update()
        {
            if (!Main.Enabled) return;
            if (!TrainController.Shared) return;
            if (_keyBinds == null) return;

            HandleMouseInputs();
            _keyInputHandler?.HandleKeyInputs();
        }

        #endregion Unity Methods

        #region Public Methods

        /// <summary>
        /// Update the key binds.
        /// </summary>
        /// <param name="keyBinds">New KeyBinds to change to.</param>
        public void UpdateKeyBinds([NotNull] KeyBinds keyBinds)
        {
            _keyBinds = keyBinds;
            _keyInputHandler?.UpdateKeyBinds(_keyBinds);
        }

        /// <summary>
        /// Update the debug mode state.
        /// </summary>
        /// <param name="state">Whether debug mode is enabled or not.</param>
        public void UpdateDebugMode(bool state)
        {
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Handles listening for clicks on cars.
        /// </summary>
        private void HandleMouseInputs()
        {
            if (Main.Settings.MouseSettings.DisableDoubleClickFollow) return;

            // bail out if the mouse button wasn't clicked this frame
            if (!Input.GetMouseButtonDown(0)) return;
            // if the time between the last click and this one is less than the threshold, it's a double click
            if (Time.time < _lastClickTime + Main.Settings.MouseSettings.DoubleClickTime)
            {
                ESLogger.LogDebug("Double click detected");
                if (!UtilityClass.TryGetCameraIfNeeded(ref _mainCamera)) return;
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                if (UtilityClass.TryGetPickableCarUnderMouse(ray, 1500, _pickableLayerMask, out Car car))
                {
                    ESLogger.LogDebug($"Double clicked car: {car.name}");
                    CameraSelector.shared.FollowCar(car);
                }
                else
                {
                    ESLogger.LogDebug("No hit detected.");
                }
            }

            // update the last click time
            _lastClickTime = Time.time;
        }

        #endregion Private Methods
    }
}