using Helpers;
using Model;
using Model.Ops;
using UI.Tags;
using UnityEngine;

namespace EasySelect.Patches;

// [HarmonyPatch(typeof(TagController), "UpdateTag")]
public class TagController_UpdateTag_Patch
{
    public static bool Prefix(Car car, TagCallout tagCallout, OpsController opsController, TagController __instance)
    {
        // proceed with the og implementation if the car is not the selected loco
        if (car != TrainController.Shared.SelectedLocomotive) return true;

        var color = Main.Settings.SelectedLocoTextColor.HexString();
        var bgColor = Main.Settings.SelectedLocoBackgroundColor;
        tagCallout.callout.Title = $"<b><color={color}>{tagCallout.callout.Title}</color></b>";
        tagCallout.callout.Text = "EasySelect";
        ApplyImageColor(tagCallout, bgColor);
        // skip the og implementation
        return false;
    }

    private static void ApplyImageColor(TagCallout tagCallout, Color color)
    {
        var colorImages = tagCallout.colorImages;
        foreach (var t in colorImages)
        {
            t.color = color;
        }
    }
}