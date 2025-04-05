using System;
using System.Reflection;
using EasySelect.Utils;
using GalaSoft.MvvmLight.Messaging;
using Game.Events;
using HarmonyLib;
using Helpers;
using JetBrains.Annotations;
using Model;
using UI;
using UnityEngine;
using Input = UnityEngine.Input;

namespace EasySelect
{
    public class EasySelectController : MonoBehaviour
    {
        #region Private Fields

        [CanBeNull] private static Harmony _harmony;

        [CanBeNull] private KeyInputHandler _keyInputHandler;

        [CanBeNull] private Camera _mainCamera;

        private float _lastClickTime;

        private static readonly int PickableLayerMask = (1 << ObjectPicker.LayerClickable) | (1 << Layers.UI) |
                                                        (1 << Layers.Default) |
                                                        (1 << Layers.Terrain);

        #endregion Private Fields

        #region Public Properties

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
            Messenger.Default.Register<MapDidLoadEvent>(this, OnMapLoaded);
        }

        private void OnDisable()
        {
            ESLogger.LogDebug("EasySelect disabled.");
            _harmony?.UnpatchAll();
            Messenger.Default.Unregister<MapDidLoadEvent>(this, OnMapLoaded);
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
            if (Main.Settings.KeyBindSettings == null)
            {
                ESLogger.LogError("KeyBinds not found.");
                return;
            }

            _keyInputHandler ??= new KeyInputHandler(Main.Settings.KeyBindSettings);
            if (_keyInputHandler == null)
            {
                ESLogger.LogError("KeyInputHandler not found.");
                return;
            }

            _harmony ??= new Harmony("EasySelect");
            if (_harmony == null)
            {
                ESLogger.LogError("Harmony not found.");
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            try // keep this try-catch bc if a patch errors, it fails silently and breaks all other patches
            {
                _harmony.PatchAll(assembly);
            }
            catch (Exception e)
            {
                ESLogger.LogError($"Error patching: {e.Message}");
                throw;
            }

            // _assetBundle ??= AssetBundle.LoadFromFile($"{Main.ModEntry.Path}{AssetBundlePath}");
            // UtilityClass.TryLoadSprites(_assetBundle, out _coupleSprite, out _uncoupleSprite);
        }

        private void Update()
        {
            if (!Main.Enabled) return;
            if (!TrainController.Shared) return;
            if (Main.Settings.KeyBindSettings == null) return;

            HandleMouseInputs();
            _keyInputHandler?.HandleKeyInputs();
        }

        #endregion Unity Methods

        #region Public Methods

        /// <summary>
        /// Update the key binds.
        /// </summary>
        /// <param name="keyBindSettings">New KeyBinds to change to.</param>
        public void UpdateKeyBinds([NotNull] KeyBindSettings keyBindSettings)
        {
            _keyInputHandler?.UpdateKeyBinds(keyBindSettings);
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
                if (!UtilityClass.TryGetCameraIfNeeded(ref _mainCamera)) return;
                if (_mainCamera == null) return;
                if (!UtilityClass.TryGetPickableCarUnderMouse(_mainCamera.ScreenPointToRay(Input.mousePosition), 1500,
                        PickableLayerMask, out var car)) return;
                HandleDoubleClickedCar(car);
            }

            // update the last click time
            _lastClickTime = Time.time;
        }

        private void HandleDoubleClickedCar(Car car)
        {
            if (GameInput.IsShiftDown)
            {
                // toggle handbrake
                car.SetHandbrake(!car.air.handbrakeApplied);
            }
            else if (GameInput.IsControlDown)
            {
                // toggle coupling
            }
            else
            {
                CameraSelector.shared.FollowCar(car);
            }
        }

        private void OnMapLoaded(MapDidLoadEvent obj) { _mainCamera = Camera.main; }

        #endregion Private Methods
    }
}