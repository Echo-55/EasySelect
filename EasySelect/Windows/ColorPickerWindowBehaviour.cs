using System;
using System.Collections.Generic;
using EasySelect.Utils;
using UI;
using UI.Builder;
using UI.Common;
using UI.Tutorial;
using UnityEngine;

namespace EasySelect.Windows;

public class ColorPickerWindowBehaviour : MonoBehaviour, IBuilderWindow
{
    private Window _window;
    private UIPanel _panel;

    private List<string> _colors = new List<string>(
    [
        "#FF0000",
        "#00FF00",
        "#0000FF",
        "#FFFF00",
        "#FF00FF",
        "#00FFFF",
        "#FFFFFF",
        "#000000"
    ]);

    private Action<string> OnColorSelected;

    public static ColorPickerWindowBehaviour Instance => WindowManager.Shared.GetWindow<ColorPickerWindowBehaviour>();
    public UIBuilderAssets BuilderAssets { get; set; }
    public bool IsVisible => _window.IsShown;

    private void Awake() { _window = GetComponent<Window>(); }

    public void Show(Action<string> onColorSelected)
    {
        OnColorSelected = onColorSelected;

        if (_panel != null)
        {
            _panel.Dispose();
            _panel = null;
        }

        Populate();
        _window.ShowWindow();
    }

    public void Hide() { _window.CloseWindow(); }

    private void Populate()
    {
        _panel = UIPanel.Create(_window.contentRectTransform, TutorialWindow.Shared.BuilderAssets, BuildWindow);

        if (_panel == null)
        {
            ESLogger.LogError("Failed to create panel.");
        }
    }

    private void BuildWindow(UIPanelBuilder panelBuilder)
    {
        _window.Title = "Color Picker";

        panelBuilder.AddColorDropdown(_colors, 0, ColorSelected);
    }

    private void ColorSelected(int colorIndex)
    {
        ESLogger.LogDebug($"Selected color: {_colors[colorIndex]}");

        OnColorSelected?.Invoke(_colors[colorIndex]);
    }
}