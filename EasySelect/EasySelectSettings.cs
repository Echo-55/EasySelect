using EasySelect.Components;
using EasySelect.Utils;
using UnityEngine;
using UnityModManagerNet;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace EasySelect;

#region Keybinds

/// <summary>
/// A class to hold the EasySelect key binds.
/// </summary>
[DrawFields(DrawFieldMask.Public)]
public class KeyBindSettings
{
    public KeyBinding SelectNextLoco = new KeyBinding { keyCode = KeyCode.RightArrow };
    public KeyBinding SelectPreviousLoco = new KeyBinding { keyCode = KeyCode.LeftArrow };
    public KeyBinding HaltTheCurrentLoco = new KeyBinding { keyCode = KeyCode.Backspace };
    public KeyBinding ReleaseAllHandBrakes = new KeyBinding { keyCode = KeyCode.Period };
    public KeyBinding ConnectAllGladhands = new KeyBinding { keyCode = KeyCode.Comma };
    public KeyBinding FollowSelectedTrain = new KeyBinding { keyCode = KeyCode.V };
    public KeyBinding JumpToLastCarDestination = new KeyBinding { keyCode = KeyCode.C };
}

#endregion Keybinds

#region Mouse Settings

/// <summary>
/// A class to hold the EasySelect mouse settings.
/// </summary>
[DrawFields(DrawFieldMask.Public)]
public class MouseSettings
{
    public bool DisableDoubleClickFollow = false;
    public float DoubleClickTime = 0.3f;
}

#endregion Mouse Settings

// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
/// <summary>
/// Class to hold EasySelect mod settings.
/// </summary>
public class EasySelectSettings : UnityModManager.ModSettings, IDrawable
{
    [Draw("Keybinds", DrawType.Auto, Collapsible = true)]
    public KeyBindSettings KeyBindSettings = new();

    [Draw("Click Settings", DrawType.Auto, Collapsible = true)]
    public MouseSettings MouseSettings = new();

    [Draw("Selected Loco Text Color", DrawType.Auto,
        Tooltip = "The color of the text in selected locomotive's tooltip.")]
    public Color SelectedLocoTextColor = Color.green;

    [Draw("Selected Loco Background Color", DrawType.Auto,
        Tooltip = "The color of the background in selected locomotive's tooltip.")]
    public Color SelectedLocoBackgroundColor = Color.clear;

    [Draw("Show FPS Display", Tooltip = "Will show the FPS in the top left corner.")]
    public bool ShowFPS = false;

    [Draw("FPS Display Position", VisibleOn = "ShowFPS|True")]
    public FPSDisplay.EPosition FPSDisplayPosition = FPSDisplay.EPosition.TopLeft;

    [Draw("Logging Level", Tooltip = "The level of logging to show.")]
    public ESLogger.ELogLevel LogLevel = ESLogger.ELogLevel.Error;

    public void OnSaveGUI() { Save(Main.ModEntry); }

    /// <summary>
    /// Called when values change.
    /// Beware, this is called a lot when updating sliders.
    /// </summary>
    public void OnChange() { }
}