using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace WayfarerKit.VFX.ScreenBlackout
{
	/// <summary>
	///     This class is used to create a screen blackout effect.
	///     It should be added as a Renderer Feature in Universal Renderer Data.
	///     The setup should be as follows:
	///     - injection point "After Rendering Post Processing"
	///     - Fetch Color Buffer is ON
	///     - Bind Depth-Stencil Buffer is OFF
	///     - Pass DrawProcedural
	/// </summary>
	public class ScreenBlackout : MonoBehaviour
    {
        private static readonly int __fadeTimeProperty = Shader.PropertyToID("_FadeTime");

        [Range(0.1f, 10f), SerializeField] private float speed = 1f;
        [SerializeField] private bool fadeOnAwake = true;
        [SerializeField] private Material fadeShaderMaterial;

        private float _lastFadeTimeValue = -1f;

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        private void Awake()
        {
            Assert.IsNotNull(fadeShaderMaterial);
            Assert.IsTrue(fadeShaderMaterial.HasProperty(__fadeTimeProperty));

            _lastFadeTimeValue = fadeShaderMaterial.GetFloat(__fadeTimeProperty);

            if (fadeOnAwake) Fade(1f, 0f);
        }

        private void OnDisable() => RestoreFadeTime();

        private void OnDestroy() => StopAllCoroutines();

        public void FadeOut(Action complete = null) => Fade(0f, 1f, complete);
        public void FadeIn(Action complete = null) => Fade(1f, 0f, complete);

        private void Fade(float from, float to, Action complete = null)
        {
            Assert.IsNotNull(fadeShaderMaterial);

            StopAllCoroutines();

            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(Interpolate(from, to, complete));
        }

        private IEnumerator Interpolate(float from, float to, Action onComplete = null)
        {
            Assert.IsTrue(from >= 0);
            Assert.IsTrue(to >= 0);

            var current = from;
            for (float t = 0; float.Epsilon < Math.Abs(current - to); t += Time.deltaTime * Speed)
            {
                current = Mathf.Clamp01(Mathf.SmoothStep(from, to, t));
                fadeShaderMaterial.SetFloat(__fadeTimeProperty, current);

                if (float.Epsilon > Math.Abs(current - to) && null != onComplete) onComplete();

                yield return null;
            }
        }

        private void RestoreFadeTime()
        {
            if (fadeShaderMaterial != null && _lastFadeTimeValue >= 0) fadeShaderMaterial.SetFloat(__fadeTimeProperty, _lastFadeTimeValue);
        }
    }
}