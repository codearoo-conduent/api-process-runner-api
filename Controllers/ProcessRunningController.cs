using Microsoft.AspNetCore.Mvc;
using api_process_runner_api.Models;
using api_process_runner_api.Helpers;
using Microsoft.SemanticKernel;
using api_process_runner_api.Util;

namespace api_process_runner_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessRunnerController : ControllerBase
    {
        private readonly ILogger<ProcessRunnerController> _logger;
        private readonly Kernel _kernel;
        public ProcessRunnerController(ILogger<ProcessRunnerController> logger, Kernel kernel)
        {
            _logger = logger;
            _kernel = kernel;
        }


        [HttpPost()] // Pass the 4 Files that were uploaded
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> LaunchProcess([FromBody] UploadedFilesRequest filesrequest)
        {
            // Hard coding these values so I do not have to manually enter them in the Swagger UI
            // This can be removed later, need this to test the SK stuff.
            filesrequest.EppicFilename = "EPPIC.20231107.CSV";
            filesrequest.SiebelFilename = "Siebel.20231107.CSV";
            filesrequest.GiactFilename = "GIACT202131107.CSV";
            filesrequest.AddressFilename = "Hospital-Shelters.20231107.csv";
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
                Console.WriteLine("Let's print out all the Hospital Records, press  Enter!");
                dataHelper.HospitalShelterDataParser.PrintHospitalRecords(hospitaldataRecords ?? new List<HospitalShelterRecords>());
                Console.WriteLine();

                // Let's test the Step 3 verification
                CallLogChecker callLogChecker = new CallLogChecker();
                // get a ref to the sibeldataRecords first
                var siebeldataRecords = dataHelper.SiebelDataRecords;
                // get a record with callnotes
                var recordswithCallNotes = dataHelper.SiebelDataParser.FindAllSiebelCallNotesByPersonID("5094334");
                var verificationsCompletedResult1 = await callLogChecker.CheckFraudIntentAsync(_kernel, recordswithCallNotes?.FirstOrDefault()?.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");

                var verificationsCompletedResult2 = await callLogChecker.CheckVerificationIntentAsync(_kernel, recordswithCallNotes?.FirstOrDefault()?.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                Console.WriteLine(verificationsCompletedResult1);

                var response = "all good";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occured: {ex.Message}");
            }
        }
    }
}
