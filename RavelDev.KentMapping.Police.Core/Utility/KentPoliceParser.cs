using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using RavelDev.KentMapping.Police.Core.Models;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using RavelDev.GoogleMaps.API;
using System.Text.Json;
using RavelDev.GoogleMaps.Models.GoogleApi;
using RavelDev.KentMapping.Police.Core.Repo;
using RavelDev.GoogleMaps.Models.GoogleApi.RavelDev.GoogleMaps.Models.GoogleApi;
using RavelDev.GoogleMaps.Utility;
using Microsoft.Extensions.Options;
using RavelDev.KentMapping.Police.Core.Models.Config;

namespace RavelDev.KentMapping.Police.Core.Utility
{
    public class KentPoliceParser
    {
        public KentPoliceParser(
            IOptions<PoliceParserConfig> parserConfig,
            GoogleMapsWebApi mapWebApi,
            KentPoliceRepository kentPoliceRepo)
        {
            ParserConfig = parserConfig.Value;
            MapWebAi = mapWebApi;
            KentPoliceRepo = kentPoliceRepo;
        }

        private PoliceParserConfig ParserConfig { get; }
        private GoogleMapsWebApi MapWebAi { get; }
        private KentPoliceRepository KentPoliceRepo { get; }

        public void GetCoordinatesForIncidents()
        {
            var incidents = KentPoliceRepo.GetAllAddressesWithoutCoordinates();
            foreach (var incident in incidents)
            {
                try
                {
                    var response = MapWebAi.GeocodeAddressRequest($"{incident.StreetAddress} Kent, OH");
                    var geocodeObj = JsonSerializer.Deserialize<GeocodeResponse>(response);

                    var firstLocationInfo = geocodeObj?.Results.FirstOrDefault()?.geometry.Location;
                    if (firstLocationInfo == null) continue;

                    KentPoliceRepo.SetCoordinatesForAddress(firstLocationInfo.Lat, firstLocationInfo.Lng, incident.AddressId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while fetching cordinates for address {incident.StreetAddress}", ex);
                }

            }

        }

        private List<string> NonCommercialPlaceTypes = new List<string>() { GooglePlaceType.Premise, GooglePlaceType.StreetAddress,
            GooglePlaceType.political, GooglePlaceType.locality, GooglePlaceType.Route
        };
        public void PopulateAddressesWithoutPlaces()
        {
            var addys = KentPoliceRepo.GetAllAddressesWithoutPlaceNames();
            addys = addys.Where(addy => addy.StreetAddress.Any(strChr => char.IsDigit(strChr))).ToList();

            foreach (var addy in addys)
            {
                try
                {
                    if (!addy.Latitude.HasValue || !addy.Longitude.HasValue) continue;

                    var response = MapWebAi.FindPlaceRequest(addy.Latitude.Value, addy.Longitude.Value);
                    var geocodeObj = JsonSerializer.Deserialize<PlacesTextSearchResponse>(response);
                    var results = geocodeObj?.Results;
                    if (results == null) continue;
                    results = results.Where(place => place.types.Any(type => !NonCommercialPlaceTypes.Contains(type))).ToList();
                    if (results.Count == 0) continue;

                    var placeName = string.Empty;
                    if (results.Count > 1)
                    {
                        placeName = string.Join(",", results.Select(place => place.name));
                        placeName = "Misc Commercial";
                    }
                    else
                    {
                        placeName = results.FirstOrDefault()?.name ?? string.Empty;

                    }

                    KentPoliceRepo.UpdatePlaceNameForAddress(placeName, addy.AddressId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while fetching place information for address id {addy.AddressId}. Street Address {addy.StreetAddress}", ex);
                }

            }
        }

        public List<KentPoliceIncident> ParseIncidentPdf(string fileLocation)
        {
            var result = new List<KentPoliceIncident>();
            using (PdfDocument document = PdfDocument.Open(fileLocation, new ParsingOptions() { ClipPaths = true }))
            {
                foreach (Page page in document.GetPages())
                {

                    var extractedPage = ObjectExtractor.Extract(document, page.Number);

                    var detector = new SimpleNurminenDetectionAlgorithm();
                    var regions = detector.Detect(extractedPage);

                    var ea = new BasicExtractionAlgorithm();
                    IReadOnlyList<Table> tables = ea.Extract(extractedPage.GetArea(regions[0].BoundingBox));

                    var table = tables[0];
                    var rows = table.Rows.ToList().GetRange(1, table.Rows.Count - 1);
                    var policeReportModels = rows.Select(row => new KentPoliceIncident(
                        dateTimeString: row[1].GetText(),
                        crNumber: row[0].GetText(),
                        incidentDescription: row[2].GetText(),
                            incidentAddress: row[3].GetText(),
                            latitude: null,
                            longitude: null)).ToList();
                    result.AddRange(policeReportModels);
                }

            }

            return result;
        }

        public List<KentPoliceCaseReport> ParseCaseReportPdf(string fileLocation)
        {
            var result = new List<KentPoliceCaseReport>();
            using (PdfDocument document = PdfDocument.Open(fileLocation, new ParsingOptions() { ClipPaths = true }))
            {
                foreach (Page page in document.GetPages())
                {
                    try
                    {
                        var extractedPage = ObjectExtractor.Extract(document, page.Number);

                        var detector = new SimpleNurminenDetectionAlgorithm();
                        var regions = detector.Detect(extractedPage);

                        var ea = new BasicExtractionAlgorithm();

                        IReadOnlyList<Table> tables = ea.Extract(extractedPage.GetArea(regions[0].BoundingBox));

                        var table = tables[0];
                        var rows = table.Rows;
                        var criminalOffense = string.Empty;
                        var multiLineOffense = false;
                        var multiLineOffenseString = string.Empty;
                        foreach (var row in rows)
                        {

                            var crNumber = row[0].GetText();

                            if (multiLineOffense)
                            {
                                criminalOffense += $" {row[2].GetText()}";
                                if (!string.IsNullOrEmpty(crNumber)) multiLineOffense = false;
                            }
                            else
                            {
                                criminalOffense = row[2].GetText();

                            }

                            if (string.IsNullOrEmpty(crNumber) && string.IsNullOrEmpty(criminalOffense))
                            {
                                continue;
                            }

                            if (string.IsNullOrEmpty(crNumber) && !string.IsNullOrEmpty(criminalOffense))
                            {
                                multiLineOffense = true;
                                continue;
                            }
                            result.Add(new KentPoliceCaseReport(
                                        crNumber: crNumber,
                                        reportedDateTime: row[1].GetText(),
                                        criminalOffense: criminalOffense,
                                        streetName: row[3].GetText(),
                                        disposition: row[4].GetText())
                                );
                            criminalOffense = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing report case file.", ex);
                    }
                }

            }

            return result;
        }

        public void ParsePendingReports()
        {

            DirectoryInfo di = new DirectoryInfo(ParserConfig.ReportsPendingDir);
            FileInfo[] files = di.GetFiles("*.pdf");


            foreach (var file in files)
            {
                try
                {
                    var reportPdfLocation = file.FullName;
                    var fileName = file.Name;
                    var reportFileId = KentPoliceRepo.InsertPoliceReportFile(fileName);
                    var reportData = ParseCaseReportPdf(reportPdfLocation);
                    KentPoliceRepo.InsertCaseReport(reportData, reportFileId);
                    File.Move(file.FullName, $"{ParserConfig.ReportsProcessedDir}\\{file.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing report file {file.FullName}", ex);
                    continue;
                }
                finally
                {
                    File.Move(file.FullName, $"{ParserConfig.IncidentsProcessedDir}\\{file.FullName}");
                }
            }
        }

        public void ParsePendingIncidents()
        {
            DirectoryInfo di = new DirectoryInfo(ParserConfig.IncidentsProcessedDir);
            FileInfo[] files = di.GetFiles("*.pdf");

            foreach (var file in files)
            {
                try
                {
                    var incidentsPdfLocation = file.FullName;
                    var fileName = file.Name;
                    var incidentFileId = KentPoliceRepo.InsertIncidentFile(fileName);
                    var incidentData = ParseIncidentPdf(incidentsPdfLocation);
                    KentPoliceRepo.InsertPoliceIncident(incidentData, incidentFileId);
                    File.Move(file.FullName, $"{ParserConfig.IncidentsProcessedDir}\\{file.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing incident file {file.FullName}", ex);
                }
                finally
                {
                    File.Move(file.FullName, $"{ParserConfig.IncidentsProcessedDir}\\{file.FullName}");
                }

            }

            GetCoordinatesForIncidents();
        }
    }
}
