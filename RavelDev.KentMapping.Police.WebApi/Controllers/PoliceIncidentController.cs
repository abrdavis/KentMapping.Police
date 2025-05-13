using Microsoft.AspNetCore.Mvc;
using RavelDev.KentMapping.Police.Core.Repo;
using RavelDev.KentMapping.WebApi.Models;

namespace RavelDev.KentMapping.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliceIncidentController : ControllerBase
    {

        private readonly ILogger<PoliceIncidentController> _logger;

        public KentPoliceRepository KentPoliceRepo { get; }

        public PoliceIncidentController(ILogger<PoliceIncidentController> logger, KentPoliceRepository kentPoliceRepo)
        {
            _logger = logger;
            KentPoliceRepo = kentPoliceRepo;
        }

        [HttpGet("GetIncidentsForTypeAndDate")]
        public IActionResult GetIncidentsForTypeAndDate(int incidentTypeId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var trafficStops = KentPoliceRepo.GetIncidentsForTypeAndDateRange(incidentTypeId, startDate, endDate);
                return new JsonResult(new { incidentData = trafficStops, success= true });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidents.");
                return new JsonResult(new { success = false});
            }
        }

        [HttpGet("GetHomePageModel")]
        public IActionResult GetHomePageModel()
        {
            try
            {

                var incidentDescriptions = KentPoliceRepo.GetIncidentDescriptions();
                var incidentDateInfo = KentPoliceRepo.GetIncidentDateInfo();
                var homeViewModel = new HomePageViewModel(incidentDescriptions, incidentDateInfo.MinDate, incidentDateInfo.MaxDate);
                return new JsonResult(new { model = homeViewModel, success=true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting page model.");
                return new JsonResult(new { success = false, ex = ex.Message});
            }
        }
    }
}