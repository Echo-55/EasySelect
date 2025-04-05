using System.Linq;
using System.Reflection;
using EasySelect.Windows;
using HarmonyLib;
using UI;
using UI.Common;
using UnityEngine;

namespace EasySelect.Patches;

[HarmonyPatch(typeof(ProgrammaticWindowCreator), "Start")]
public class ProgrammaticWindowCreator_Start_Patch
{
    private static readonly MethodInfo _createWindowMethodInfo = AccessTools
        .GetDeclaredMethods(typeof(ProgrammaticWindowCreator))
        .FirstOrDefault(m =>
            m.Name == "CreateWindow" &&
            m.GetParameters().Length == 6 &&
            m.IsGenericMethod);

    public static void Postfix(ProgrammaticWindowCreator __instance)
    {
        var genericMethod = _createWindowMethodInfo.MakeGenericMethod(typeof(ColorPickerWindowBehaviour));
        genericMethod.Invoke(__instance, new object[]
        {
            "ColorPicker",
            250,
            350,
            Window.Position.Center,
            Window.Sizing.Resizable(new Vector2Int(200, 300)),
            null
        });
    }
}