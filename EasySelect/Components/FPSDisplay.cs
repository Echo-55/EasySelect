using UnityEngine;

namespace EasySelect.Components;

public class FPSDisplay : MonoBehaviour
{
    private float _deltaTime = 0.0f; // Time between frames
    private EPosition _position = EPosition.TopRight;

    public enum EPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private void Update()
    {
        // Accumulate the time it took to render the last frame
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        // Calculate FPS
        var fps = 1.0f / _deltaTime;
        var text = $"{fps:0.} FPS";

        // Define style for the FPS display
        var style = new GUIStyle
        {
            fontSize = 24,
            normal =
            {
                textColor = Color.white
            }
        };

        // Add a shadow for better visibility
        var shadowStyle = new GUIStyle(style)
        {
            normal =
            {
                textColor = Color.black
            }
        };

        // Position the text
        var margin = 10f;
        Rect rect = _position switch
        {
            EPosition.TopLeft => new Rect(margin, margin, 150, 50),
            EPosition.TopRight => new Rect(Screen.width - 150 - margin, margin, 150, 50),
            EPosition.BottomLeft => new Rect(margin, Screen.height - 50 - margin, 150, 50),
            EPosition.BottomRight => new Rect(Screen.width - 150 - margin, Screen.height - 50 - margin, 150, 50),
            _ => new Rect(Screen.width - 150 - margin, margin, 150, 50)
        };

        // Draw shadow text first, slightly offset
        GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, shadowStyle);
        // Draw main text
        GUI.Label(rect, text, style);
    }

    public void UpdatePosition(EPosition position) { _position = position; }
}