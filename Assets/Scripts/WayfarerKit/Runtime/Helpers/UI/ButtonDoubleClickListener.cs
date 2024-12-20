using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WayfarerKit.Helpers.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonDoubleClickListener : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Duration between 2 clicks in seconds"), Range(0.01f, 0.5f)]
         public float doubleClickDuration = 0.4f;
        public UnityEvent onDoubleClick;
        private Button _button;

        private byte _clicks;
        private DateTime _firstClickTime;

        private void Awake() => _button = GetComponent<Button>();

        public void OnPointerClick(PointerEventData eventData)
        {
            var elapsedSeconds = (DateTime.Now - _firstClickTime).TotalSeconds;
            if (elapsedSeconds > doubleClickDuration)
                _clicks = 0;

            _clicks++;

            switch (_clicks)
            {
                case 1:
                    _firstClickTime = DateTime.Now;
                    break;
                case > 1:
                {
                    if (elapsedSeconds <= doubleClickDuration)
                    {
                        if (_button.interactable)
                            onDoubleClick?.Invoke();
                    }
                    _clicks = 0;
                    break;
                }
            }
        }
    }
}