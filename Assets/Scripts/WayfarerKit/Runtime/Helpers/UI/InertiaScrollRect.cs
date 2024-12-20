using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WayfarerKit.Helpers.UI
{
    public class InertiaScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler
    {
        private const float ScrollStopThreshold = 0.05f;
        
        public float maxOverdrag = 50;
        private bool _draggingByTouch;

        private bool _isDragging;

        private bool _isScrollStopped;

        private float _lastScrollTime;
        private Vector2 _originalPosition;
        private bool _overdragging;

        private float _previousScrollDelta = float.MinValue;

        private Vector2 _prevPosition = Vector2.zero;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            _draggingByTouch = eventData.pointerId != -1;
            _isDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            _overdragging = false;
            base.OnEndDrag(eventData);
            _isDragging = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_overdragging) base.OnDrag(eventData);

            if (!vertical) return;
            
            if (content.anchoredPosition.y < _originalPosition.y - maxOverdrag)
            {
                _overdragging = true;
                content.anchoredPosition = new(content.anchoredPosition.x, _originalPosition.y - maxOverdrag);
            }
            else
                _overdragging = false;
        }


        public override void Rebuild(CanvasUpdate executing)
        {
            base.Rebuild(executing);

            if (executing == CanvasUpdate.PostLayout) _prevPosition = content.anchoredPosition;
        }

#region Unity Methods
        protected override void Awake()
        {
            base.Awake();

            _originalPosition = content.anchoredPosition;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopAllCoroutines();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _isDragging = false;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            var deltaTime = Time.unscaledDeltaTime;
            if (deltaTime > 0.0f && _isDragging && inertia)
            {
                Vector3 newVelocity = (content.anchoredPosition - _prevPosition) / deltaTime;
                velocity = _draggingByTouch ? newVelocity * .8f : Vector3.Lerp(velocity, newVelocity, deltaTime * 10f);
            }

            _prevPosition = content.anchoredPosition;

            if (Application.isPlaying)
            {
                // scroll movement check
                if (velocity.magnitude > ScrollStopThreshold)
                {
                    _lastScrollTime = Time.time;
                    _isScrollStopped = false;
                }
                else if (!_isScrollStopped) _isScrollStopped = true;
            }
        }
#endregion
    }
}