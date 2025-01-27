using System.Collections;
using System.Linq;
using System.Reflection;
using Game.Messages;
using HarmonyLib;
using Model;
using Model.AI;
using UI;
using UI.Common;
using UI.EngineControls;
using UnityEngine;

namespace EasySelect.Utils;

public class KeyInputHandler(KeyBinds keyBinds)
{
    private KeyBinds _keyBinds = keyBinds;

    private FieldInfo _persistenceFieldInfo;

    public void HandleKeyInputs()
    {
        if (Input.GetKeyDown(_keyBinds.SelectNextLoco))
        {
            UtilityClass.SelectNextLoco(GameInput.IsShiftDown);
        }
        else if (Input.GetKeyDown(_keyBinds.SelectPreviousLoco))
        {
            UtilityClass.SelectPreviousLoco(GameInput.IsShiftDown);
        }

        HandleHaltCurrentLocoInput();
        HandleReleaseHandbrakesInput();
        HandleConnectGladHandsInput();
        HandleFollowCarInput();
        HandleJumpToLastCarDestinationInput();
    }

    public void UpdateKeyBinds(KeyBinds keyBinds)
    {
        _keyBinds = keyBinds;
    }

    private void HandleHaltCurrentLocoInput()
    {
        if (!Input.GetKeyDown(_keyBinds.HaltTheCurrentLoco)) return;

        BaseLocomotive currentLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!currentLoco)
        {
            ESLogger.LogDebugError("No locomotive selected.");
            return;
        }

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
        IEnumerator noticeFromLocoCo =
            UtilityClass.PostTempNoticeFromLocoCO(currentLoco, "es-halt", "Halted by user.", 5f);
        currentLoco.StartCoroutine(noticeFromLocoCo);
        UtilityClass.ShowToast($"Clearing waypoint for: {currentLoco.DisplayName}", ToastPosition.Bottom);
    }

    private void HandleReleaseHandbrakesInput()
    {
        if (!Input.GetKeyDown(_keyBinds.ReleaseAllHandBrakes)) return;

        var connectedCars = UtilityClass.GetCurrentlySelectedTrainCars();
        var enumerable = connectedCars as Car[] ?? connectedCars.ToArray();
        if (!enumerable.Any())
        {
            ESLogger.LogDebugError("No connected cars found.");
            return;
        }

        foreach (Car car in enumerable)
        {
            car.SetHandbrake(false);
        }

        ESLogger.LogDebug("Released all handbrakes.");

        BaseLocomotive currentlySelectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        currentlySelectedLoco?.StartCoroutine(
            UtilityClass.PostTempNoticeFromLocoCO(currentlySelectedLoco, "es-release",
                "handbrakes released.", 3f));
    }

    private void HandleConnectGladHandsInput()
    {
        if (!Input.GetKeyDown(_keyBinds.ConnectAllGladhands)) return;

        ESLogger.LogDebug("Connecting glad hands.");

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
        {
            ESLogger.LogDebugError("No locomotive selected.");
            return;
        }

        var carList = selectedLoco.EnumerateCoupled().ToList();
        TrainController.ConnectCars(carList);
    }

    private void HandleFollowCarInput()
    {
        if (!Input.GetKeyDown(_keyBinds.FollowSelectedTrain)) return;

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
        {
            ESLogger.LogDebugError("No locomotive selected.");
            return;
        }

        CameraSelector.shared.FollowCar(selectedLoco);
    }

    private void HandleJumpToLastCarDestinationInput()
    {
        if (!Input.GetKeyDown(_keyBinds.JumpToLastCarDestination)) return;

        BaseLocomotive selectedLoco = UtilityClass.GetCurrentlySelectedLoco();
        if (!selectedLoco)
        {
            ESLogger.LogDebugError("No locomotive selected.");
            return;
        }

        Car lastCar = UtilityClass.GetLastCar(selectedLoco);
        if (!lastCar)
        {
            ESLogger.LogDebugError("No cars found.");
            return;
        }

        ESLogger.LogDebug($"Last car found: {lastCar.name}");

        if (UtilityClass.TryGetDestinationInfo(lastCar, out Vector3 destinationPosition))
        {
            CameraSelector.shared.ZoomToPoint(destinationPosition);
        }
        else
        {
            ESLogger.LogDebugError("Destination not found.");
        }
    }
}