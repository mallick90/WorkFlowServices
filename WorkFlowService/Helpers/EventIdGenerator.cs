namespace WorkFlowService.Helpers
{
    public class EventIdGenerator
    {
        private static int _counter = 0;
        private static readonly object _lock = new object();

        public static string GenerateEventId()
        {
            lock (_lock)
            {
                _counter++;
                return $"event-{_counter}-{DateTime.UtcNow.Ticks}";
            }
        }
    }
}
