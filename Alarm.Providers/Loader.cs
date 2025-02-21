using Alarm.Loader;

namespace Alarm.Providers
{
    public class Loader : ModLoader
    {
        protected override void Load()
        {
            base.Load();

            RegisterProviderBuilder("fw", new(Source.FloatingWeight.Managed.FromJson, typeof(Source.FloatingWeight.Managed.Config)));

            RegisterProviderBuilder("pf", new(Pipeline.PossibilityFilter.FromJson, typeof(Pipeline.PossibilityFilter.Config)));
        }
    }
}
