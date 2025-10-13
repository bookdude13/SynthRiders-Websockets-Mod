using MelonLoader;
using System.Security.Cryptography;

namespace SynthRidersWebsockets
{
    /// <summary>
    /// Used to ensure that melon loader versions of needed dependencies are up to date.
    /// This is a stopgap until https://github.com/LavaGang/MelonLoader/issues/1000 is resolved.
    /// </summary>
    internal class FixMelonLoaderDllVersions
    {
        public static void EnsureDependencies(MelonLogger.Instance logger)
        {
            EnsureDependency(logger, "Microsoft.Extensions.Configuration.dll", "3D5D8FDDC11B27573942D1FEBA5D8AC91E2504C82AD489003ACF3E17FDB130EC");
            EnsureDependency(logger, "Microsoft.Extensions.DependencyInjection.Abstractions.dll", "901B7C5727F29FA4F55EB2CF9F1A44D15AFD02F1D14103EB08966DE31F5BB12F");
        }

        /// <summary>
        /// If the given dll is present from this mod but isn't updated in the MelonLoader folder,
        /// i.e. the MelonLoader version is the known old hash, replace it with the mod's updated version.
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="outOfDateSha256">SHA256, from Get-FileHash, of the known old MelonLoader dll</param>
        public static void EnsureDependency(MelonLogger.Instance logger, string dllName, string outOfDateSha256)
        {
            // Get locations of the DLLs
            var synthRidersDir = Directory.GetCurrentDirectory(); // current directory returns the SynthRiders directory
            logger.Msg("SR dir is " + synthRidersDir);

            // Updated DLLs are packaged with the plugin w/ .disabled extension so MelonLoader doesn't think they're plugins as well
            var newDllDir = Path.Combine(synthRidersDir, "Plugins");

            if (!Directory.Exists(newDllDir))
                return;

            var melonLoaderDllsPath = Path.Combine(Directory.GetParent(newDllDir).FullName, "MelonLoader", "net6");
            logger.Msg("MelonLoader dll path is " + melonLoaderDllsPath);

            if (!Directory.Exists(melonLoaderDllsPath))
                return;

            // Get full DLL paths and make sure they actually exist
            var updatedDllPath = Path.Combine(newDllDir, dllName + ".disabled");
            var melonLoaderDllPath = Path.Combine(melonLoaderDllsPath, dllName);

            logger.Msg("modDllPath is " + updatedDllPath);
            if (!File.Exists(updatedDllPath))
            {
                logger.Msg("Mod dll not found at '" + updatedDllPath + "'");
                return;
            }

            logger.Msg("melonLoaderDllPath is " + melonLoaderDllPath);
            if (!File.Exists(melonLoaderDllPath))
            {
                logger.Msg("Melon loader dll not found at '" + melonLoaderDllPath + "'");
                return;
            }

            // Get the old file's hash. If it's the old one, we'll need to copy.
            var melonLoaderDllHash = CalculateSha256(logger, melonLoaderDllPath);
            logger.Msg("Hash is " + melonLoaderDllHash);

            if (melonLoaderDllHash == null)
            {
                logger.Msg("Hash not calculated");
                return;
            }

            if (melonLoaderDllHash != outOfDateSha256)
            {
                logger.Msg("Not a match; skipping");
                return;
            }

            logger.Msg("Hash match; trying to replace MelonLoader DLL with newer version");

            // Save off old version (changing extension so it doesn't load), then replace with new one
            File.Copy(melonLoaderDllPath, melonLoaderDllPath + ".OLD", true);
            File.Copy(updatedDllPath, melonLoaderDllPath, true);
        }

        /// <summary>
        /// Reads file from path, calculates SHA-256 and returns as a hex string
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string CalculateSha256(MelonLogger.Instance logger, string path)
        {
            try
            {
                using (SHA256 mySHA256 = SHA256.Create())
                {
                    using (FileStream fileStream = File.OpenRead(path))
                    {
                        // Create a fileStream for the file.
                        // Be sure it's positioned to the beginning of the stream.
                        fileStream.Position = 0;

                        // Compute the hash of the fileStream.
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);

                        return Convert.ToHexString(hashValue);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to calculate hash: {ex.Message}");
            }

            return null;
        }
    }
}
