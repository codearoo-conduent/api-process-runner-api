using Microsoft.AspNetCore.Mvc;
using api_process_runner_api.Models;
using api_process_runner_api.Helpers;

namespace api_process_runner_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessRunnerController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ProcessRunnerController> _logger;

        public ProcessRunnerController(ILogger<ProcessRunnerController> logger)
        {
            _logger = logger;
        }


        [HttpPost()] // Pass the 4 Files that were uploaded
        [ProducesResponseType(typeof(UploadFileResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> LaunchProcess([FromBody] UploadedFilesRequest filesrequest)
        {
            /* Sample test request
{
    "giactFilename": "GIACT202131107.CSV",
    "siebelFilename": "Siebel.20231107.CSV",
    "eppicFilename": "EPPIC.20231107.CSV",
    "addressFilename": "Hospital-Shelters.20231107.csv"
}
             */

            try
            {
                if (filesrequest == null)
                {
                    return BadRequest(filesrequest);
                }
                var _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");
                // Load the Data
                // Siebel.20231107.CSV
                // Hospital-Shelters.20231107.csv
                // GIACT202131107.CSV
                // EPPIC.20231107.CSV
                DataHelper dataHelper = new DataHelper(filesrequest, _blobConnection, true);
                var result = await dataHelper.Intialize();
                var hospitaldataRecords = dataHelper.HospitalDataRecords;
                Console.WriteLine("Ready to Go, let's search for a HospitalByFullAddress. Press Enter!");
                var recordswithFullAddress = dataHelper.HospitalShelterDataParser.FindHospitalByFullAddress("799 47dH bd", "", "SAN DIEGO", "CA", hospitaldataRecords ?? new List<HospitalShelterRecords>());
                Console.WriteLine($@"Hospital Found: {recordswithFullAddress?.AddressLine1}");
                Console.WriteLine("Let's print out all the Hospital Records");
                //dataHelper.HospitalShelterDataParser.PrintHospitalRecords(hospitaldataRecords ?? new List<HospitalShelterRecords>());
                Console.WriteLine();

                var response = new UploadFileResponse() 
                { OutputFilename = $"FraudCheckOutput-{DateTime.UtcNow.ToString("yyyyMMdd")}-{Guid.NewGuid()}.json" };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occured: {ex.Message}");
            }
        }
    }
}
