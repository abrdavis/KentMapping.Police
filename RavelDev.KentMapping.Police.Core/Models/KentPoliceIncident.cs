namespace RavelDev.KentMapping.Police.Core.Models
{
    public class KentPoliceIncident
    {
        public int KentPoliceIncidentId { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string CrNumber { get; set; }
        public string IncidentDescription { get; set; }
        public string IncidentAddress { get; set; }
        public string CaseReportDescription { get; set; }
        public string DateDisplayString => IncidentDate?.ToString("g") ?? string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }



        public KentPoliceIncident()
        {
            CrNumber = string.Empty;
            IncidentDescription = string.Empty;
            IncidentAddress = string.Empty;
            IncidentDate = null;
            CaseReportDescription = string.Empty;
        }

        public KentPoliceIncident(string dateTimeString,
            string crNumber, 
            string incidentDescription,
            string incidentAddress,
            double? latitude,
            double? longitude)
        {
            DateTime tempDateTime;
            var success = DateTime.TryParse(dateTimeString, out tempDateTime);
            IncidentDate = success ? tempDateTime : null;
            CrNumber = crNumber;
            IncidentDescription = incidentDescription;
            IncidentAddress = incidentAddress;
            Latitude = latitude;
            Longitude = longitude;
            CaseReportDescription = string.Empty;
        }
    }
}
