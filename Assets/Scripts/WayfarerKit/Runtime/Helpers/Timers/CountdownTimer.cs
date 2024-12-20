namespace WayfarerKit.Helpers.Timers
{
    public class CountdownTimer : Timer
    {
        public CountdownTimer(float value) : base(value) {}

        public bool IsFinished => Time <= 0f;

        public override void Tick(float deltaTime)
        {
            if (IsRunning && Time > 0f) Time -= deltaTime;
            if (IsRunning && Time <= 0f) Stop();
        }

        public void Reset() => Time = InitialTime;

        public void Reset(float newTime)
        {
            InitialTime = newTime;
            Reset();
        }
    }
}