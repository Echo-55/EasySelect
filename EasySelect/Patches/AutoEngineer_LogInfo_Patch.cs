using HarmonyLib;
using Model.AI;

namespace EasySelect.Patches;

[HarmonyPatch(typeof(AutoEngineer), "LogInfo")]
public class AutoEngineer_LogInfo_Patch
{
    public static bool Prefix(AutoEngineer __instance, string message)
    {
        // skip logging the message
        return false;
    }
}