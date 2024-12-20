using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WayfarerKit.Helpers.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonLongPressListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("Hold duration in seconds"), Range(0.3f, 5f)]
         public float holdDuration = 0.5f;
        public UnityEvent onLongPress;
        private Button _button;
        private WaitForSeconds _delay;
        private bool _isLongPressed;

        private bool _isPointerDown;
        private DateTime _pressTime;


        private void Awake()
        {
            _button = GetComponent<Button>();
            _delay = new(0.1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _pressTime = DateTime.Now;
            StartCoroutine(Timer());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPointerDown = false;
            _isLongPressed = false;
        }

        private IEnumerator Timer()
        {
            while (_isPointerDown && !_isLongPressed)
            {
                var elapsedSeconds = (DateTime.Now - _pressTime).TotalSeconds;

                if (elapsedSeconds >= holdDuration)
                {
                    _isLongPressed = true;
                    if (_button.interactable)
                        onLongPress?.Invoke();

                    yield break;
                }

                yield return _delay;
            }
        }
    }
}