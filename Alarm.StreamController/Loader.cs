using Alarm.Loader;
namespace Alarm.StreamController
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();
            RegisterPostHandler("controller.console", new(ConsoleController.FromJson, typeof(bool)));
        }
    }
}
