using EasySelect.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Model;
using RollingStock;
using UI.ContextMenu;
using UnityEngine;
using ContextMenu = UI.ContextMenu.ContextMenu;

namespace EasySelect.Patches;

/// <summary>
/// Adds extra functionality to the context menu of a car.
/// </summary>
[HarmonyPatch(typeof(CarPickable), "HandleShowContextMenu")]
public class CarPickable_HandleShowContextMenu_Patch
{
    [CanBeNull] private static TrainController _trainController;
    [CanBeNull] private static ContextMenu _contextMenu;

    private static Sprite _uncoupleSprite;
    private static Sprite _coupleSprite;

    public CarPickable_HandleShowContextMenu_Patch()
    {
        _uncoupleSprite = EasySelectController.UncoupleSprite;
        _coupleSprite = EasySelectController.CoupleSprite;
    }

    public static void Prefix(Car car, CarPickable __instance)
    {
        // TODO: broken
        // the way the ContextMenu is implemented, it is built in quadrants, with each being a button
        // it is set up in a way there can be less than 4 and it adjusts the layout accordingly, but over 4 breaks it
        // new layout? new page?
        return;

        if (!TryGetReferences(out _trainController, out _contextMenu))
            return;

        _uncoupleSprite ??= EasySelectController.UncoupleSprite;
        if (_uncoupleSprite == null)
        {
            ESLogger.LogError("uncouple sprite is null");
            return;
        }

        _coupleSprite ??= EasySelectController.CoupleSprite;
        if (!_coupleSprite)
        {
            ESLogger.LogError("couple sprite is null");
            return;
        }

        Car.EndGear aGear = car.EndGearA;
        bool aIsCoupled = aGear.IsCoupled;
        Coupler aCoupler = aGear.Coupler;
        bool aIsFront = car.FrontIsA;


        Car.EndGear bGear = car.EndGearB;
        bool bIsCoupled = bGear.IsCoupled;
        Coupler bCoupler = bGear.Coupler;
        bool bIsFront = !car.FrontIsA;

        Car.EndGear frontGear = aIsFront ? aGear : bGear;
        bool frontGearIsCoupled = frontGear.IsCoupled;
        Coupler frontCoupler = aIsFront ? aCoupler : bCoupler;

        Car.EndGear rearGear = aIsFront ? bGear : aGear;
        bool rearGearIsCoupled = rearGear.IsCoupled;
        Coupler rearCoupler = aIsFront ? bCoupler : aCoupler;

        // Options:
        // 1. Button disconnects/connects both the coupler and the air hoses
        // - Two buttons: connect/disconnect all, one for each end of the car

        // 2. Conditionally show button for each based on whether they are coupled
        // - Four buttons: connect/disconnect for both couplers and air hoses, two for each end of the car

        // 1. No buttons if the car is not coupled
        // 2. Only need one button if the car is coupled at one end
        // - Determine which end is coupled and add button for that end
        // 3. If both ends are coupled, add buttons for both ends

        if (!aIsCoupled && !bIsCoupled)
            return;

        if (frontGearIsCoupled && rearGearIsCoupled)
        {
            _contextMenu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect Front", _uncoupleSprite,
                () => { frontCoupler?.SetOpen(true); });

            _contextMenu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect Rear", _uncoupleSprite,
                () => { rearCoupler?.SetOpen(true); });
        }
        else if (frontGearIsCoupled)
        {
            _contextMenu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect", _uncoupleSprite, () =>
            {
                frontCoupler?.SetOpen(true);
                frontGearIsCoupled = false;
            });
        }
        else if (rearGearIsCoupled)
        {
            _contextMenu.AddButton(ContextMenuQuadrant.Unused2, "Disconnect", _uncoupleSprite, () =>
            {
                rearCoupler?.SetOpen(true);
                rearGearIsCoupled = false;
            });
        }
    }

    [ContractAnnotation(
        "=> true, trainController: notnull, contextMenu: notnull; => false, trainController: null, contextMenu: null")]
    private static bool TryGetReferences(out TrainController trainController, out ContextMenu contextMenu)
    {
        trainController = _trainController ?? TrainController.Shared;
        contextMenu = _contextMenu ?? ContextMenu.Shared;

        if (!trainController)
        {
            ESLogger.LogError("TrainController is null.");
            return false;
        }

        if (!contextMenu)
        {
            ESLogger.LogError("ContextMenu is null.");
            return false;
        }

        _trainController = trainController;
        _contextMenu = contextMenu;
        return true;
    }
}