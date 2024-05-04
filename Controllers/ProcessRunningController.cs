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
        private readonly UploadedFilesRequest? _filesrequest;
        private readonly bool _debugging = true;
        public ProcessRunnerController(ILogger<ProcessRunnerController> logger, Kernel kernel, UploadedFilesRequest uploadedfilesrequest)
        {
            _logger = logger;
            _kernel = kernel;
            _filesrequest = uploadedfilesrequest;
        }


        [HttpGet("StartProcessing")] // Pass the 4 Files that were uploaded
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> ProcessRunner()
        {
            // we need to clear out all the collections after each completion otherwise the collection will get larger as long as the process lives.
            // TBD
            try
            {
                var _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");

                DataHelper dataHelper;
                
                if (_filesrequest != null) {
                    dataHelper = new DataHelper(_filesrequest , _blobConnection, true);
                    dataHelper?.ClearCollections(); // Clear All collections just to make sure data does not linger across runs.
                }
                else
                {
                    return BadRequest("Issue with File Detals!");
                }
                var result = await dataHelper.Intialize();
                var hospitaldataRecords = dataHelper.HospitalDataRecords;
                Console.WriteLine("Ready to Go, let's search for a HospitalByFullAddress. Press Enter!");
                var recordswithFullAddress = dataHelper.HospitalShelterDataParser.FindHospitalByFullAddress("799 47dH bd", "", "SAN DIEGO", "CA", hospitaldataRecords ?? new List<HospitalShelterRecords>());
                Console.WriteLine($@"Hospital Found: {recordswithFullAddress?.AddressLine1}");
                Console.WriteLine("Let's print out all the Hospital Records, press  Enter!");
                dataHelper.HospitalShelterDataParser.PrintHospitalRecords(hospitaldataRecords ?? new List<HospitalShelterRecords>());
                Console.WriteLine();

                var eppicdataRecords = dataHelper.EppicDataRecords;
                Console.WriteLine("Ready to Go, let's search for a Eppic PersonID. Press Enter!");
                var recordMatchedPersonID = dataHelper.EppicDataParser.FindEppicPersonID("5094334", eppicdataRecords ?? new List<EppicRecords>());
                Console.WriteLine($@"PersonID Found: {recordMatchedPersonID?.PersonID}");
                Console.WriteLine("Let's print out all the Eppic Records, press  Enter!");
                dataHelper.EppicDataParser.PrintEppicRecords(eppicdataRecords ?? new List<EppicRecords>());
                Console.WriteLine();

                // Let's test the Step 3 verification
                CallLogChecker callLogChecker = new CallLogChecker();
                // get a ref to the sibeldataRecords first
                var siebeldataRecords = dataHelper.SiebelDataRecords;
                // get a record with callnotes
                var recordswithCallNotes = dataHelper.SiebelDataParser.FindAllSiebelCallNotesByPersonID("5094334");
                var verificationsCompletedResult1 = await callLogChecker.CheckFraudIntentAsync(_kernel, recordswithCallNotes?.FirstOrDefault()?.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");

                var verificationsCompletedResult2 = await callLogChecker.CheckVerificationIntentAsync(_kernel, recordswithCallNotes?.FirstOrDefault()?.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");
                Console.WriteLine(verificationsCompletedResult2);

                // Test the Step Logger  Let's Add the Eppic Items that Failed Step 1
                // There are not Eppic Items that have a Match in Hospital DB so no need to test
                // So there are no records that match Hospital Address so nothing will be added!
                StepLogger stepLogger = new StepLogger();
                if (Globals.inputEppicRecordsNotInHospitalDB != null)
                {
                    var eppicRecordsNotInHospitalDB = Globals.inputEppicRecordsNotInHospitalDB.ToList();
                    foreach (var record in eppicRecordsNotInHospitalDB)
                    {
                        // TBD Needs to be debugged it's printing out like 20 items when there are only 5
                        stepLogger.AddItem(record, "Step 1 - Eppic Records Not in Hospital List", "FAIL Go to next Step");
                    }
                }
                // stepLogger.TestAddItems();  // This will add 10 test items to the Logger Collection
                stepLogger.PrintItems();

                var response = "all good";  // this needs to be fixed.
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occured: {ex.Message}");
            }
        }

        [HttpPost("SendFileDetails")] // Pass the 4 Files that were uploaded
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public ActionResult SendFileDetails([FromBody] UploadedFilesRequest filesrequest)
        {
            if (filesrequest != null && _filesrequest != null && filesrequest.SiebelFilename !="string")
            { // This only gets executed if the request body has the file details, otherwise it uses the defaults from the DI in the program.cs
 
                _filesrequest.EppicFilename = filesrequest.EppicFilename;
                _filesrequest.SiebelFilename = filesrequest.SiebelFilename;
                _filesrequest.GiactFilename = filesrequest.GiactFilename;
                _filesrequest.AddressFilename = filesrequest.AddressFilename;
            } // Otherwise we assume the local files in the program.cs should be used.
            
            try
            {
                if (filesrequest == null)
                {
                    return BadRequest(filesrequest);
                }
                LogFileGenerator.GenerateLogFileName();  // return generated filename to client  TBD add DI for this so it can be shared across async functions and it needs to be cached.

                var response = LogFileGenerator.GenerateLogFileName();
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occured: {ex.Message}");
            }


        }
    }
}
