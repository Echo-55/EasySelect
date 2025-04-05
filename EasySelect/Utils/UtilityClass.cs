using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Game.Notices;
using JetBrains.Annotations;
using Model;
using Model.AI;
using Model.Ops;
using RollingStock;
using Track.Search;
using UI.Common;
using UI.EngineRoster;
using UnityEngine;
using ZLinq;

namespace EasySelect.Utils;

public static class UtilityClass
{
    /// <summary>
    /// Posts a temporary notice from the provided locomotive.
    /// Top right message boxes that can be clicked away.
    /// Resending the same key will overwrite the previous message.
    /// Empty message will clear the notice.
    /// </summary>
    /// <param name="loco">The loco which will run the coroutine</param>
    /// <param name="key">Key for the notice</param>
    /// <param name="message">Message for the notice</param>
    /// <param name="displayTime">How long to display the notice</param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static IEnumerator PostTempNoticeFromLocoCO(BaseLocomotive loco, string key, string message,
        float displayTime)
    {
        NoticeManager noticeManager = NoticeManager.Shared;
        if (!noticeManager)
        {
            ESLogger.LogError("NoticeManager not found.");
            yield break;
        }

        noticeManager.PostEphemeral(new EntityReference(EntityType.Car, loco.id), key, message);
        yield return new WaitForSeconds(displayTime);
        noticeManager.PostEphemeral(new EntityReference(EntityType.Car, loco.id), key, "");
    }

    /// <summary>
    /// Shows a toast message at the specified position.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="toastPosition">The position in which to show the toast notification</param>
    public static void ShowToast(string message, ToastPosition toastPosition) { Toast.Present(message, toastPosition); }

    public static bool TryGetTrainController(out TrainController trainController)
    {
        trainController = TrainController.Shared;
        if (trainController) return true;
        ESLogger.LogError("TrainController not found.");
        return false;
    }

    /// <summary>
    /// Selects a locomotive by index in which the roster panel displays them.
    /// Index is 0-based.
    /// </summary>
    /// <param name="index">Index in the roster panel of the loco to select</param>
    /// <param name="alsoJumpTo">Also set the camera to follow the loco</param>
    private static void SelectLoco(int index, bool alsoJumpTo)
    {
        if (index < 0)
        {
            ESLogger.LogError("Index cannot be negative.");
            return;
        }

        var ownedLocos = GetOwnedLocos();
        if (index >= ownedLocos.Count)
        {
            ESLogger.LogError($"Index {index} is out of bounds for owned locomotives.");
            return;
        }

        EngineRosterPanel engineRosterPanel = EngineRosterPanel.Shared;
        if (!engineRosterPanel)
        {
            ESLogger.LogError("EngineRosterPanel not found.");
            return;
        }

        engineRosterPanel.SelectEngine(ownedLocos[index], true);

        CameraSelector cameraSelector = CameraSelector.shared;
        if (alsoJumpTo && cameraSelector)
        {
            cameraSelector.FollowCar(ownedLocos[index]);
        }
    }

    /// <summary>
    /// Based on the currently selected locomotive, selects the next one in the roster panel.
    /// </summary>
    /// <param name="alsoJumpTo">Also set the camera to follow the loco</param>
    public static void SelectNextLoco(bool alsoJumpTo = false)
    {
        ESLogger.LogDebug("SelectNextLoco() called.");

        var ownedLocos = GetOwnedLocos();
        if (ownedLocos.Count == 0)
        {
            ESLogger.LogError("No owned locomotives found.");
            return;
        }

        if (!TryGetTrainController(out var trainController))
        {
            ESLogger.LogError("TrainController not found.");
            return;
        }

        if (trainController.SelectedLocomotive == null)
        {
            ESLogger.LogError("No selected locomotive found.");
            return;
        }

        if (ownedLocos.Count == 1)
        {
            ESLogger.LogDebug("Only one locomotive owned, nothing to select.");
            return;
        }

        // Get the currently selected locomotive and select the next one
        BaseLocomotive currentLoco = trainController.SelectedLocomotive;
        var nextIndex = (ownedLocos.IndexOf(currentLoco) + 1) % ownedLocos.Count;
        SelectLoco(nextIndex, alsoJumpTo);
    }

    /// <summary>
    /// Based on the currently selected locomotive, selects the previous one in the roster panel.
    /// </summary>
    /// <param name="alsoJumpTo">Also set the camera to follow the loco</param>
    public static void SelectPreviousLoco(bool alsoJumpTo = false)
    {
        ESLogger.LogDebug("SelectPreviousLoco() called.");

        var ownedLocos = GetOwnedLocos();
        if (ownedLocos.Count == 0)
        {
            ESLogger.LogError("No owned locomotives found.");
            return;
        }

        if (!TryGetTrainController(out var trainController))
        {
            ESLogger.LogError("TrainController not found.");
            return;
        }

        if (trainController.SelectedLocomotive == null)
        {
            ESLogger.LogError("No selected locomotive found.");
            return;
        }

        if (ownedLocos.Count == 1)
        {
            ESLogger.LogDebug("Only one locomotive owned, nothing to select.");
            return;
        }

        // Get the currently selected locomotive and select the previous one
        BaseLocomotive currentLoco = trainController?.SelectedLocomotive ?? ownedLocos[0];
        var prevIndex = (ownedLocos.IndexOf(currentLoco) - 1 + ownedLocos.Count) % ownedLocos.Count;
        SelectLoco(prevIndex, alsoJumpTo);
    }

    /// <summary>
    /// Gets all the locomotives owned by the player.
    /// </summary>
    /// <returns>A list of owned locomotives</returns>
    public static List<BaseLocomotive> GetOwnedLocos()
    {
        if (TryGetTrainController(out var trainController))
            return trainController.Cars
                .AsValueEnumerable()
                .OfType<BaseLocomotive>()
                .Where(loco => loco.IsOwnedByPlayer)
                .ToList();
        ESLogger.LogError("TrainController not found.");
        return new List<BaseLocomotive>();
    }

    /// <summary>
    /// Returns all Cars connected to the currently selected locomotive.
    /// </summary>
    /// <returns>A list of connected cars</returns>
    public static IEnumerable<Car> GetCurrentlySelectedTrainCars()
    {
        TrainController trainController = TrainController.Shared;
        if (trainController) return trainController.SelectedTrain;
        ESLogger.LogError("TrainController not found.");
        return new List<Car>();
    }

    /// <summary>
    /// Tries to load the couple and uncouple sprites from the provided asset bundle.
    /// </summary>
    /// <param name="assetBundle"></param>
    /// <param name="coupleSprite"></param>
    /// <param name="uncoupleSprite"></param>
    public static void TryLoadSprites(AssetBundle assetBundle, out Sprite coupleSprite, out Sprite uncoupleSprite)
    {
        coupleSprite = assetBundle.LoadAsset<Sprite>("couple");
        if (!coupleSprite)
        {
            ESLogger.LogError("Failed to load couple sprite.");
        }

        uncoupleSprite = assetBundle.LoadAsset<Sprite>("uncouple");
        if (!uncoupleSprite)
        {
            ESLogger.LogError("Failed to load uncouple sprite.");
        }
    }

    /// <summary>
    /// Tries to get the main camera if the provided one is null.
    /// </summary>
    /// <param name="camera"></param>
    /// <returns></returns>
    [ContractAnnotation("=> true, camera: notnull; => false, camera: null")]
    public static bool TryGetCameraIfNeeded(ref Camera camera)
    {
        if (!camera)
        {
            camera = Camera.main;
        }

        return camera;
    }

    /// <summary>
    /// Tries to get the car under the mouse cursor.
    /// </summary>
    /// <param name="ray">Ray to use for the raycast.</param>
    /// <param name="distance">Distance to extend the raycast.</param>
    /// <param name="layerMask">Layer mask for the raycast.</param>
    /// <param name="car">Out reference to hit car, if there is one.</param>
    /// <returns></returns>
    public static bool TryGetPickableCarUnderMouse(Ray ray, float distance, int layerMask, out Car car)
    {
        car = null;
        if (!Physics.Raycast(ray, out RaycastHit hit, distance, layerMask))
        {
            ESLogger.LogDebug("No hit detected.");
            return false;
        }

        var compInParent = hit.collider.gameObject.GetComponentInParent<IPickable>();
        if (compInParent == null)
        {
            ESLogger.LogDebug("No pickable found.");
            return false;
        }

        ESLogger.LogDebug($"Hit pickable: {hit.transform.parent.name}");

        if (compInParent is not CarPickable carPickable)
        {
            ESLogger.LogDebug("Hit pickable but not a car pickable.");
            return false;
        }

        ESLogger.LogDebug($"Hit car pickable: {carPickable.name}");

        car = carPickable.car;
        if (car)
        {
            ESLogger.LogDebug($"Car: {car.name}");
            return true;
        }

        ESLogger.LogDebug("Hit car pickable but car is null.");
        return false;
    }

    /// <summary>
    /// Gets the currently selected locomotive.
    /// </summary>
    /// <returns>The currently selected loco if there is one</returns>
    public static BaseLocomotive GetCurrentlySelectedLoco()
    {
        TrainController trainController = TrainController.Shared;
        return trainController?.SelectedLocomotive;
    }

    /// <summary>
    /// Get the last car in the selected train.
    /// Skips locomotives and tenders.
    /// </summary>
    /// <param name="locomotive"></param>
    /// <returns></returns>
    public static Car GetLastCar(BaseLocomotive locomotive)
    {
        var enumerateCoupled = locomotive.EnumerateCoupled(Car.LogicalEnd.B).AsValueEnumerable();
        if (!enumerateCoupled.Any())
        {
            ESLogger.LogDebugError("EnumerateCoupled() returned no cars.");
            return null;
        }

        Car lastCar = enumerateCoupled.LastOrDefault();
        if (lastCar == null)
        {
            ESLogger.LogDebugError("Last car is null.");
            return null;
        }

        // var enumerable = enumerateCoupled as Car[] ?? enumerateCoupled.ToArray();
        // if (!enumerable.Any())
        // {
        //     ESLogger.LogDebugError("EnumerateCoupled() returned no cars.");
        //     return null;
        // }
        //
        // Car lastCar = enumerable.LastOrDefault();
        //
        // // TODO: There probably already exists a method for this in the game code
        // // if the last car is the selected loco, or the selected loco's tender wagon, we have the wrong end of the train
        // // so get the car from the other end
        // var selectedSteamLocomotive = locomotive as SteamLocomotive;
        // if (selectedSteamLocomotive && IsCarATenderOrLoco(lastCar, selectedSteamLocomotive))
        // {
        //     lastCar = enumerable.FirstOrDefault();
        // }

        return lastCar;
    }

    /// <summary>
    /// Checks if the last car is a tender or locomotive.
    /// </summary>
    /// <param name="lastCar">Last car in a train set</param>
    /// <param name="loco">The loco to check</param>
    /// <returns></returns>
    public static bool IsCarATenderOrLoco(Car lastCar, BaseLocomotive loco)
    {
        var locoIsSteam = loco is SteamLocomotive;
        if (!locoIsSteam) return false;
        var locoHasTender =
            loco.TryGetAdjacentCar(loco.EndToLogical(Car.End.R), out Car tender);
        return locoHasTender && (lastCar == loco || lastCar == tender);
    }

    /// <summary>
    /// Tries to get the destination info of the car
    /// </summary>
    /// <param name="car">The car to get the destination info for</param>
    /// <param name="destinationPosition">Out reference to the destination position</param>
    /// <returns></returns>
    public static bool TryGetJobDestinationInfoForCar(Car car, out Vector3 destinationPosition)
    {
        destinationPosition = Vector3.zero;
        OpsController opsController = OpsController.Shared;
        if (!opsController)
        {
            ESLogger.LogDebugError("OpsController not found.");
            return false;
        }

        if (!opsController.TryGetDestinationInfo(car, out var destinationName, out var isAtDestination,
                out destinationPosition, out OpsCarPosition _)) return false;

        ESLogger.LogDebug($"Destination found: {destinationName}");
        ESLogger.LogDebug($"Destination position found: {destinationPosition}");
        ESLogger.LogDebug($"Is at destination: {isAtDestination}");

        return true;
    }

    /// <summary>
    /// Tries to get the AE waypoint for the locomotive.
    /// </summary>
    /// <param name="locomotive">The loco to check</param>
    /// <param name="aeWaypointPosition">Out reference to the world position</param>
    /// <returns>Whether an AE waypoint position was found or not</returns>
    public static bool TryGetAEWaypointForLoco(BaseLocomotive locomotive, out Vector3 aeWaypointPosition)
    {
        FieldInfo routeField = typeof(AutoEngineerPlanner).GetField("_route",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (routeField == null)
        {
            ESLogger.LogDebugError("Field _route not found.");
            aeWaypointPosition = Vector3.zero;
            return false;
        }

        aeWaypointPosition = Vector3.zero;
        AutoEngineerPlanner aePersistence = locomotive.AutoEngineerPlanner;
        var steps = (List<RouteSearch.Step>)routeField.GetValue(aePersistence);
        if (steps == null || steps.Count == 0)
        {
            ESLogger.LogDebugError("No steps found.");
            return false;
        }

        if (steps.Count == 1)
        {
            ESLogger.LogDebug("Only one step found.");
            aeWaypointPosition = steps[0].Location.GetPosition();
            return true;
        }

        RouteSearch.Step lastStep = steps[^1];
        aeWaypointPosition = lastStep.Location.GetPosition();
        return true;
    }
}