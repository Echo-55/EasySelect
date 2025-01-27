using System.Reflection;
using HarmonyLib;
using Model;
using UI;
using UI.EngineRoster;

namespace EasySelect.Patches;

/// <summary>
/// If holding shift when clicking the jump to button, also select the engine.
/// </summary>
[HarmonyPatch(typeof(EngineRosterRow), "ActionJumpTo")]
public class EngineRosterPanel_Populate_Patch
{
    private static readonly FieldInfo EngineField = AccessTools.Field(typeof(EngineRosterRow), "_engine");

    public static void Postfix(EngineRosterRow __instance)
    {
        var loco = EngineField.GetValue(__instance) as BaseLocomotive;
        if (loco == null) return;
        if (GameInput.IsShiftDown)
        {
            EngineRosterPanel.Shared.SelectEngine(loco, true);
        }
    }
}