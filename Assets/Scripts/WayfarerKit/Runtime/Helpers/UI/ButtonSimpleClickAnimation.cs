using PrimeTween;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using WayfarerKit.Helpers.Serialization;

namespace WayfarerKit.Helpers.UI
{
    public class ButtonSimpleClickAnimation : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField, Range(0.1f, 10f)] private float frequency = .5f;
        [SerializeField] private Optional<Transform> customAnimationRoot;

        private Transform _cachedTransform;
        
        private void Awake()
        {
            _cachedTransform = customAnimationRoot.Enabled ? customAnimationRoot.Value : transform;
            
            if (button == null)
                button = gameObject.GetComponentInParent<Button>();

            Assert.IsNotNull(button, "button != null");

            button.onClick.AddListener(AnimateHandler);
        }

        private void OnDestroy()
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(AnimateHandler);
        }

        private void AnimateHandler()
        {
            if (_cachedTransform.gameObject is { activeSelf: true, activeInHierarchy: true })
                Tween.PunchScale(_cachedTransform, Vector3.one, .2f, frequency);
        }
    }
}