using Alarm.Loader;

namespace Alarm.Providers
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();

            RegisterProviderBuilder("fw", Source.FloatingWeight.Managed.FromJson);

            RegisterProviderBuilder("pf", Pipeline.PossibilityFilter.FromJson);
        }
    }
}
