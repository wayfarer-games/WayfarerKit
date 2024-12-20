using UnityEngine.Assertions;
using WayfarerKit.Patterns.EventBus;

namespace WayfarerKit.Systems.SceneManagement.Helpers
{
    public class BootLoadingProgressEvent : IBusEvent
    {
        private BootLoadingProgressEvent() {}

		/// <summary>
		///     Value from 0 to 1
		///     Step is logical part of the loading process
		/// </summary>
		public float Progress { get; private set; }
        public (int Index, int Count) Steps { get; private set; }

        public static BootLoadingProgressEvent From(float totalProgress, int stepIndex, int stepCount)
        {
            Assert.IsTrue(totalProgress >= 0);
            Assert.IsTrue(totalProgress <= 1);
            Assert.IsTrue(stepIndex >= 0 && stepCount > 0);
            Assert.IsTrue(stepIndex < stepCount);

            return new()
            {
                Progress = totalProgress,
                Steps = (stepIndex, stepCount)
            };
        }
    }
}