using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Logging;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace WayfarerKit.Helpers
{
    [DisallowMultipleComponent]
    public class DragThresholdAdjuster : MonoBehaviour
    {
        [SerializeField] private EventSystem eventSystem;

        private void Awake()
        {
            Assert.IsNotNull(eventSystem);
        }

        private void Start()
        {
            var defaultValue = eventSystem.pixelDragThreshold;		
            
            // “Drag Threshold” is updated to “15” pixels for Samsung Galaxy S5 which is a 480 DPI device.
            // Magical number 160 is the accepted (1) DPI value for medium sized screen devices.
            var dpi =  Mathf.Max(defaultValue , (int) (defaultValue * Screen.dpi / 160f));
            eventSystem.pixelDragThreshold = dpi;

            Log.Debug($"DragThresholdAdjuster -> <color=yellow>DPI: {Screen.dpi}</color>, PixelDragThreshold: <color=yellow>{dpi}</color>");
        }
    }
}