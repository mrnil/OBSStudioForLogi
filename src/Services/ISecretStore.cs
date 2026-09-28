namespace Loupedeck.OBSStudioForLogiPlugin.Services
{
    using System;

    // Encrypted storage for values that must not sit in plaintext in config.json, such as the
    // remote OBS password. The plugin backs this with the SDK's plugin settings, which the Logi
    // Plugin Service stores encrypted on both Windows and macOS.
    public interface ISecretStore
    {
        Boolean TryGet(String name, out String value);

        void Set(String name, String value);

        void Delete(String name);
    }
}
