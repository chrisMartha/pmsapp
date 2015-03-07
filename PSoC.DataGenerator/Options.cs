using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace PSoC.DataGenerator
{
    public class Options
    {
        [Option('d', "districts", Required = false,
            HelpText = "Districts to be added")]
        public string DistrictsToAdd { get; set; }

        [Option('r', "requests", DefaultValue = 0,
            HelpText = "license requests to be made")]
        public int LicenseRequestToAdd { get; set; }

        [Option('c', "count", DefaultValue = 0,
          HelpText = "number of districts to add.")]
        public int NumberOfDistrictsToAdd { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
