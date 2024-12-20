using UnityEngine;
using UnityEngine.UI;

namespace WayfarerKit.Helpers.UI
{
    public static class SafeAreaIndent
    {
        public static float CalculateVerticalIndent(Canvas canvas)
        {
            var indent = Screen.safeArea.y;
            if (Mathf.Approximately(indent, 0f))
                return 0f;
            
            var canvasScale = canvas.GetComponent<CanvasScaler>();
            var referenceResolution = canvasScale.referenceResolution;
            var screenResolution = new Vector2(Screen.width, Screen.height);

            var result= indent / Mathf.Max(
                Mathf.Lerp(screenResolution.x / referenceResolution.x, screenResolution.y / referenceResolution.y, canvasScale.matchWidthOrHeight), 
                1f);
            return result * 1f;
        }
    }
}