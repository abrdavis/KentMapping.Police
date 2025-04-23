using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelDev.KentMapping.PoliceLogParser
{
    public class CLArguments
    {
        [Option("mapaddy", Required = false)]
        public bool MapAddress { get; set; }

        [Option("reports", Required = false)]
        public bool ParseReports { get; set; }

        [Option("incidents", Required = false)]
        public bool ParseIncidents { get; set; }

        [Option('a', "parseall", Required = false)]
        public bool ParseAll { get; set; }
    }
}
