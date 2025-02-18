namespace Alarm.Core
{
    public static class EventHelper
    {
        public static void DoNothing(object? sender, EventArgs e) { }

        public static EventHandler DoAction(Action action)
        {
            return (_, _) =>
            {
                action.Invoke();
            };
        }

        public static EventHandler Wait(TimeSpan t)
        {
            return (_, _) =>
            {
                Thread.Sleep(t);
            };
        }

        public static EventHandler OnlyOnce(EventHandler handler, int limit = 1)
        {
            int i = limit;
            return (s, e) =>
            {
                if (i > 0)
                {
                    i--;
                    handler(s, e);
                }
            };
        }
    }
}
