using System;
using UnityEngine;

namespace WayfarerKit.UI.Standard
{
    public abstract class ProgressLerpElement : MonoBehaviour, IProgress<float>
    {
        private const float FinalFillSpeed = 20f;

        [Range(1f, 10f), SerializeField, Tooltip("How fast the progress bar fills up.")]
        private float progressFillSpeed = 5f;

        private float _progress;

        protected abstract float ViewFillAmount { get; set; }

        protected virtual void Update()
        {
            var difference = Mathf.Abs(ViewFillAmount - _progress);
            var dynamicFillSpeed = Mathf.Approximately(_progress, 1f)
                ? FinalFillSpeed
                : Mathf.Pow(difference, 2) * progressFillSpeed;

            ViewFillAmount = Mathf.Lerp(ViewFillAmount, _progress, dynamicFillSpeed * Time.deltaTime);
        }

        protected virtual void OnEnable() => ResetProgress();

        public void Report(float value) => _progress = Mathf.Clamp01(value);
        private void ResetProgress() => _progress = ViewFillAmount = 0f;
    }
}