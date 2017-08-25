using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Shared.Config
{
    /// <summary> class used to validate asset creation file commands </summary>
    public class AssetFileValidator
    {
        public static AssetFileValidator Instance { get; set; } = new AssetFileValidator("AllowedFiletypes.txt");

        private string[] validExtensions;
        private Action<string>[] Validators;

        private string GetExtension(string filename) => filename.Split('.').Last();
        private void Die(string message) => throw new InvalidOperationException($"Asset filename {message}");

        private Action<string> MakeValidator(Func<string, bool> condition, string message) => (filename) => {
            if (!condition(filename))
                Die(message);
        };

        /// <summary> validate a filename against extensions, throw on failure. </summary>
        /// <param name="filename"></param>
        public void ValidateFileExtension(string filename)
        {
            foreach(var validate in Validators)
                validate(filename);
        }

        /// <summary></summary>
        /// <param name="configFile">The path to a file containing valid file extensions </param>
        public AssetFileValidator(string configFile)
        {
            if (!File.Exists(configFile))
                throw new InvalidOperationException($"asset file extensions config not found: {configFile}");

            // assume that every line is an extension, not globbing or doing anything fancy for now.
            validExtensions = File.ReadAllLines(configFile).Where(line => !string.IsNullOrEmpty(line)).ToArray();

            Validators = new[] {
                MakeValidator((filename) => !string.IsNullOrEmpty(filename), "is null or empty"),
                MakeValidator((filename) => filename.Contains("."), "has no extensions found"),

                MakeValidator((filename) => validExtensions.Contains(GetExtension(filename)), 
                    $"extension not within allowed values: " + string.Join(", ", validExtensions))
            };
        }
    }
}
