using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RavelDev.KentMapping.Police.Core.Models
{
    public class KentPoliceCaseReport
    {
        public KentPoliceCaseReport(string crNumber, string reportedDateTime, string criminalOffense, string streetName, string disposition)
        {
            DateTime tempDateTime;
            var success = DateTime.TryParse(reportedDateTime, out tempDateTime);
            ReportedDateTime = success ? tempDateTime : null;
            CrNumber = crNumber;
            ReportedDateTimeString = reportedDateTime;
            CriminalOffense = criminalOffense;
            StreetName = streetName;
            Disposition = disposition;
        }

        public string CrNumber { get; set; }
        public DateTime? ReportedDateTime { get; set; }

        public string CriminalOffense { get; set; }
        public string Disposition { get; set; }
        public string StreetName { get; set; }
        public string ReportedDateTimeString { get; }
    }
}
