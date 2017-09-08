namespace Squidex.Infrastructure.Assets
{
    using System.IO;
    using System.Linq;

    /// <summary> class used to validate asset creation file commands </summary>
    // TODO: consider finding a better place for this?
    public static class AssetFileValidationConfig
    {
        public static string[] ValidExtensions { get; } =
            File.ReadAllLines( "AllowedFiletypes.txt")
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(ext => ext.ToLower())
                .ToArray();
    }
}
