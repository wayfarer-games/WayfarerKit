using WayfarerKit.Patterns.EventBus;

namespace WayfarerKit.Systems.SceneManagement.Helpers.BusEvents
{
    public class BootDumpPointEvent : IBusEvent
    {
        public string EventGroup { get; protected set; }
        
        private BootDumpPointEvent() {}
        public static BootDumpPointEvent With(string value) => new()
        {
            EventGroup = value
        };
    }
}