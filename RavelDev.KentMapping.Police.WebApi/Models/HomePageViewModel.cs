using RavelDev.KentMapping.Police.Core.Models;

namespace RavelDev.KentMapping.WebApi.Models
{
    public class HomePageViewModel
    {
        public HomePageViewModel()
        {
            IncidentDescriptions = new List<PoliceIncidentModel>();
        }
        public HomePageViewModel(List<PoliceIncidentModel> incidentDescriptions)
        {
            IncidentDescriptions = incidentDescriptions;
        }

        public HomePageViewModel(List<PoliceIncidentModel> incidentDescriptions, DateTime minDate, DateTime maxDate) : this(incidentDescriptions)
        {
            MinDate = minDate;
            MaxDate = maxDate;
        }

        public List<PoliceIncidentModel> IncidentDescriptions { get; }
        public DateTime MinDate { get; }
        public DateTime MaxDate { get; }
    }
}
