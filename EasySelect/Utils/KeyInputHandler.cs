using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Messages;
using Game.State;
using HarmonyLib;
using Model;
using Model.AI;
using UI.Common;
using UI.EngineControls;
using UnityEngine;
using UnityModManagerNet;
using ZLinq;

namespace EasySelect.Utils;

public class KeybindAction(KeyBinding keyBinding, Action action)
{
    public void HandleKeyInput()
    {
        if (keyBinding.Down())
            action?.Invoke();
    }
}

public class KeyInputHandler
{
    private KeyBindSettings _keyBindSettings;

    private FieldInfo _persistenceFieldInfo;

    private List<KeybindAction> _keybindActions;

    private readonly Dictionary<KeyCode, AutoEngineerMode> _aeModeKeybinds = new()
    {
        { KeyCode.Keypad1, AutoEngineerMode.Off },
        { KeyCode.Keypad2, AutoEngineerMode.Road },
        { KeyCode.Keypad3, AutoEngineerMode.Yard },
        { KeyCode.Keypad4, AutoEngineerMode.Waypoint }
    };

    public KeyInputHandler(KeyBindSettings keyBindSettings)
    {
        _keyBindSettings = keyBindSettings;
        InitializeKeybindActions();
    }

    private void InitializeKeybindActions()
    {
        _keybindActions =
        [
            new KeybindAction(_keyBindSettings.HaltTheCurrentLoco, HandleHaltCurrentLocoInput),
            new KeybindAction(_keyBindSettings.ReleaseAllHandBrakes, HandleReleaseHandbrakesInput),
            new KeybindAction(_keyBindSettings.ConnectAllGladhands, HandleConnectGladHandsInput),
            new KeybindAction(_keyBindSettings.FollowSelectedTrain, HandleFollowCarInput),
            new KeybindAction(_keyBindSettings.JumpToLastCarDestination, HandleJumpToLastCarDestinationInput)
        ];
    }

    public void HandleKeyInputs()
    {
        foreach (KeybindAction keybindAction in _keybindActions)
        {
            keybindAction.HandleKeyInput();
        }

        HandleChangingSelectedLocosAEMode();
    }

    public void UpdateKeyBinds(KeyBindSettings keyBindSettings) { _keyBindSettings = keyBindSettings; }

    private void HandleHaltCurrentLocoInput()
    {
        if (!Main.Settings.KeyBindSettings.HaltTheCurrentLoco.Down()) return;

        BaseLocomotive currentLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!currentLoco)
            return;

        AutoEngineerPlanner aePlanner = currentLoco.AutoEngineerPlanner;
        if (!aePlanner)
        {
            ESLogger.LogDebugError("AutoEngineerPlanner not found.");
            return;
        }

        _persistenceFieldInfo ??= AccessTools.Field(typeof(AutoEngineerPlanner), "_persistence");
        if (_persistenceFieldInfo == null)
        {
            ESLogger.LogError("FieldInfo not found.");
            return;
        }

        if (_persistenceFieldInfo.GetValue(aePlanner) is not AutoEngineerPersistence persistence)
        {
            ESLogger.LogError("AutoEngineerPersistence not found.");
            return;
        }

        Orders orders = persistence.Orders;

        if (orders.Mode != AutoEngineerMode.Waypoint)
            return;

        AutoEngineerWaypointControls waypointControls = AutoEngineerWaypointControls.Shared;
        if (!waypointControls)
        {
            ESLogger.LogError("AutoEngineerWaypointControls not found when attempting to halt a loco.");
            return;
        }

        waypointControls.DidClickStop();
        var playerName = StateManager.Shared.PlayersManager.LocalPlayer.Name;
        IEnumerator noticeFromLocoCo =
            UtilityClass.PostTempNoticeFromLocoCO(currentLoco, "es-halt", $"AE waypoint cleared by {playerName}.", 5f);
        currentLoco.StartCoroutine(noticeFromLocoCo);
        UtilityClass.ShowToast($"Clearing waypoint for: {currentLoco.DisplayName}", ToastPosition.Bottom);
    }

    private void HandleReleaseHandbrakesInput()
    {
        if (!Main.Settings.KeyBindSettings.ReleaseAllHandBrakes.Down()) return;

        var connectedCars = UtilityClass.GetCurrentlySelectedTrainCars();
        var enumerable = connectedCars.AsValueEnumerable();
        foreach (Car car in enumerable)
        {
            car.SetHandbrake(false);
        }

        ESLogger.LogDebug("Released all handbrakes.");

        BaseLocomotive currentlySelectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        currentlySelectedLoco?.StartCoroutine(
            UtilityClass.PostTempNoticeFromLocoCO(currentlySelectedLoco, "es-release",
                "Handbrakes released.", 3f));
    }

    private void HandleConnectGladHandsInput()
    {
        // if (!Input.GetKeyDown(_keyBindSettings.ConnectAllGladhands)) return;
        if (!Main.Settings.KeyBindSettings.ConnectAllGladhands.Down()) return;

        ESLogger.LogDebug("Connecting glad hands.");

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
            return;

        var carList = selectedLoco.EnumerateCoupled().AsValueEnumerable().ToList();
        TrainController.ConnectCars(carList);
    }

    private void HandleFollowCarInput()
    {
        // if (!Input.GetKeyDown(_keyBindSettings.FollowSelectedTrain)) return;
        if (!Main.Settings.KeyBindSettings.FollowSelectedTrain.Down()) return;

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
            return;

        CameraSelector.shared.FollowCar(selectedLoco);
    }

    private void HandleJumpToLastCarDestinationInput()
    {
        // if (!Input.GetKeyDown(_keyBindSettings.JumpToLastCarDestination)) return;
        if (!Main.Settings.KeyBindSettings.JumpToLastCarDestination.Down()) return;

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
            return;

        Car lastCar = UtilityClass.GetLastCar(selectedLoco);
        if (!lastCar)
        {
            ESLogger.LogDebugError("No cars found.");
            return;
        }

        ESLogger.LogDebug($"Last car found: {lastCar.name}");

        // if the last car has a job destination, zoom to that
        if (UtilityClass.TryGetJobDestinationInfoForCar(lastCar, out Vector3 destinationPosition))
        {
            CameraSelector.shared.ZoomToPoint(destinationPosition);
        }
        else
        {
            ESLogger.LogDebugError("Destination not found.");
        }

        // if the last car doesn't have a job, try jumping to the loco's AE waypoint
        if (UtilityClass.TryGetAEWaypointForLoco(selectedLoco, out Vector3 waypointPosition))
        {
            CameraSelector.shared.ZoomToPoint(waypointPosition);
        }
        else
        {
            ESLogger.LogDebugError("Waypoint not found.");
        }
    }

    private void HandleChangingSelectedLocosAEMode()
    {
        var selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
            return;

        foreach (var kvp in _aeModeKeybinds)
        {
            if (Input.GetKeyDown(kvp.Key))
            {
                ESLogger.LogDebug($"Changing AE mode to {kvp.Value}");
                var command = new AutoEngineerCommand(selectedLoco.id, kvp.Value, true, 35, 0, null, null);
                StateManager.ApplyLocal(command);
                break;
            }
        }
    }
}