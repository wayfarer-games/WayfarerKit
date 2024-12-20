using System;

namespace WayfarerKit.Helpers.Timers
{
    public abstract class Timer
    {
        public Action OnTimerStart = delegate {};
        public Action OnTimerStop = delegate {};

        protected Timer(float value)
        {
            InitialTime = value;
            IsRunning = false;
        }
        protected float InitialTime { get; set; }

        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }

        public float Progress => Time / InitialTime;

        public void Start()
        {
            Time = InitialTime;

            if (IsRunning) return;

            IsRunning = true;
            OnTimerStart.Invoke();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;

            OnTimerStop.Invoke();
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(float deltaTime);
    }
}