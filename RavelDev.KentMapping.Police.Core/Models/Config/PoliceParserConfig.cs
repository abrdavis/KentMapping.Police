using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelDev.KentMapping.Police.Core.Models.Config
{
    public class PoliceParserConfig
    {
        public required string ReportsPendingDir { get; set; }
        public required string IncidentsPendingDir { get; set; }
        public required string ReportsProcessedDir { get; set; }
        public required string IncidentsProcessedDir { get; set; }
    }
}
