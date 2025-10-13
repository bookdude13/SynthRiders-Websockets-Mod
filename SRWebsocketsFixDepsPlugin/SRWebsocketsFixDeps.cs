using MelonLoader;
using SynthRidersWebsockets;

namespace SRWebsocketsFixDepsPlugin
{
    public class SRWebsocketsFixDeps : MelonPlugin
    {
        public override void OnPreInitialization()
        {
            base.OnPreInitialization();

            FixMelonLoaderDllVersions.EnsureDependencies(LoggerInstance);
        }
    }
}
