using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Shared.Config
{
    /// <summary> class used to validate asset creation file commands </summary>
    // TODO: consider finding a better place for this?
    public static class AssetFileValidationConfig
    {
        public static string[] ValidExtensions { get; } =
            File.ReadAllLines( "AllowedFileTypes.txt")
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(ext => ext.ToLower())
                .ToArray();
    }
}
