using System;

namespace WayfarerKit.Systems.SceneManagement.Helpers
{
    public sealed class LoadingProgressReport : IProgress<float>
    {
        private const float Ratio = 1f;

        public void Report(float value) => Progressed?.Invoke(value / Ratio);
        public event Action<float> Progressed;
    }
}