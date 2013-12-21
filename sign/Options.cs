using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Sign
{
    internal class Options
    {
        [OptionArray('f', "files", HelpText = "List of files to sign")]
        public string[] Files { get; set; }

        [OptionArray('d', "dirs", HelpText = "List of directories contains files to sign")]
        public string[] Dirs { get; set; }

        [Option('k', "key", Required = true, HelpText = "*.snk file contains public key.")]
        public string Key { get; set; }

        [Option('p', "filter", Required = false, DefaultValue = "(dll|exe)$", HelpText = "File selection filter")]
        public string Pattern { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Display information messages.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
