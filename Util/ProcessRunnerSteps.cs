using api_process_runner_api.Helpers;
using api_process_runner_api.Models;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.SemanticKernel;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace api_process_runner_api.Util
{
   
    public class StepResult
    {
        public string? StepName { get; set; }
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
    }

    public class ProcessResult
    {
        public StepResult? EppicAddressInHospitalDBResult { get; set; }  // Step 1
        public StepResult? EppicAddressInGiactResult { get; set; }       // Step 2 
        public StepResult? EppicSeibelCallNotesResult { get; set; }      // Step 3
        public StepResult? EppicSeibelOPTCallNotesResult { get; set; }   // Step 3a
        public string? MasterStatusCode { get; set; }                    // Pass or Fail
    }
    //  public class ProcessRunnerSteps(EppicRecords eppicrecord, SiebelDataParser siebeldataparser, GiactDataParser giactdataparser, EppicDataParser eppicdataparser)
    internal class ProcessRunnerSteps(DataHelper datahelper, Kernel kernel, JobStatus jobstatus, StepsLogFile stepslogfile)
    {
        private DataHelper _datahelper = datahelper;
        private Kernel _kernel = kernel;
        private JobStatus _jobstatus = jobstatus;
        private StepsLogFile _stepslogfile = stepslogfile;
        //private EppicRecords _eppicrecord = eppicrecord;
        //private SiebelDataParser _siebeldataparser = siebeldataparser;
        //private GiactDataParser _giactdataparser = giactdataparser;
        //private EppicDataParser _eppicdataparser = eppicdataparser;

        //  public ProcessResult RunSteps()
        public async Task RunSteps()
        {
            // var processResult = new ProcessResult();
            #region Step 1: Eppic Check Against Hospital DB - no need to run step 1
            StepLogger stepLogger = new StepLogger();
            if (Globals.inputEppicRecordsInHospitalDB != null && (Globals.inputEppicRecordsInHospitalDB.Count() > 0))
            {
                // Let's add the items that have a match in Hospital DB to the Log Collection
                var eppicRecordsNotInHospitalDB = Globals.inputEppicRecordsInHospitalDB.ToList();
                foreach (var record in eppicRecordsNotInHospitalDB)
                {
                    // TBD Needs to be debugged it's printing out like 20 items when there are only 5
                    stepLogger.AddItem(record, "Step 1 - Eppic Records in Hospital List", "PASS/Stop do not go to next step");
                }
            }
            if (Globals.inputEppicRecordsNotInHospitalDB != null && (Globals.inputEppicRecordsNotInHospitalDB.Count() > 0))
            {
                // Let's add the items that do not have match Hospital DB to the Log Collection
                var eppicRecordsNotInHospitalDB = Globals.inputEppicRecordsNotInHospitalDB.ToList();
                foreach (var record in eppicRecordsNotInHospitalDB)
                {
                    // TBD Needs to be debugged it's printing out like 20 items when there are only 5
                    stepLogger.AddItem(record, "Step 1 - Eppic Records Not in Hospital List", "FAIL Go to next Step");
                }
            }
            _jobstatus.Status = "Step 1 completed";
            #endregion

            #region Step 2: Check Eppic Address against GIACT only if last step is set to Pass
            // Now let's process the items from Step 1 that did not have a match in the Hospital DB
            // Check the Address Information in GIACT 
            // In the DataHelper.Initilize() when we load the data we build a list of Epic items that have an exact match in Giact and list that does not have a match
            // Now, all we have to do is log the result to the Log collection!

            if (Globals.inputEppicRecordsInGiactDB != null && (Globals.inputEppicRecordsInGiactDB.Count() > 0))
            {
                // Let's add the items that have a match in Hospital DB to the Log Collection
                var eppicRecordsInGiatDB = Globals.inputEppicRecordsInGiactDB.ToList();
                foreach (var record in eppicRecordsInGiatDB)
                {
                    stepLogger.AddItem(record, "Step 2 - Eppic Records in Giact DB", "Match/Contine to step 3");
                }
            }
            if (Globals.inputEppicRecordsNotInGiactDB != null && (Globals.inputEppicRecordsNotInGiactDB.Count() > 0))
            {
                // Let's add the items that do not have match Hospital DB to the Log Collection
                var eppicRecordsNotInGiactDB = Globals.inputEppicRecordsNotInGiactDB.ToList();
                foreach (var record in eppicRecordsNotInGiactDB)
                {
                    stepLogger.AddItem(record, "Step 2 - Eppic Records Not in Giact DB", "Denied, marked as Fraud no need to process");
                }
            }
            _jobstatus.Status = "Step 2 completed";

            #endregion

            #region Step 3: Check Eppic Record Against Seibel CallNotesResult only if last step is set to pass
            // TBD.  Here I would like to add the additional JSON data from the SK logic to the data we are logging to the Collection
            // Need to look into how to add optional JSON data to the structure
            // Let's run the step 3 verification

            CallLogChecker callLogChecker = new CallLogChecker();

            // We need to loop through all the items that have a match in Giact DB for step 3 now
            // This logic could be moved into a function/method to simplify this section of code.
            if (Globals.inputEppicRecordsInGiactDB != null && (Globals.inputEppicRecordsInGiactDB.Count() > 0))
            {
                // Let's add the items that have a match in Hospital DB to the Log Collection
                var eppicRecordsInGiatDB = Globals.inputEppicRecordsInGiactDB.ToList();
                foreach (var record in eppicRecordsInGiatDB)
                {
                    // get a ref to the sibeldataRecords first
                    //var siebeldataRecords = _datahelper.SiebelDataRecords;
                    var recordswithCallNotes = datahelper.SiebelDataParser.FindAllSiebelCallNotesByPersonID(record.PersonID ?? "");
                    var verificationsCompletedJson = await callLogChecker.CheckVerificationIntentAsync(_kernel, recordswithCallNotes?.FirstOrDefault()?.PersonID ?? "", recordswithCallNotes?.FirstOrDefault()?.CallNotes ?? "");

                    VerificationCompleted? verificationcompleted = JsonSerializer.Deserialize<VerificationCompleted>(verificationsCompletedJson);

                    if (verificationcompleted?.VerificationsCompleted == "Yes")
                    {
                        // No need to move to step 3.a if verification has been completed
                        // TBD I would like to actually log the JSON of the verificationcompleted to the collection 
                        stepLogger.AddItem(record, "Step 3 - Eppic Record Passed Verification Check", "PASS Verification Check no need to move to next step!");

                        #region step 3a
                        // the above needs to include OTP as well; i the SIEBEL call notes list the form of authentication as "One Time Passcode",
                        // then we need to ensure that hte phone # in the call notes matches the EPICC record phone # OR the phone number in GIACT
                        if (verificationcompleted?.FormOfAuthentication == "one time passcode")
                        {
                            bool step3aPass = false;
                            step3aPass = _datahelper.Step3a_Check(verificationcompleted.PhoneNumber, record);

                            // if the check doesn't pass, the request is determined to be fraud
                            if (!step3aPass)
                            {
                                stepLogger.AddItem(record, "Step 3a - OTP Pass identified but phone number does not match the phone number in EPPIC or GIACT", "FAIL - fraudelant request");
                            }
                        }
                        #endregion
                    }
                    else  // Verifications have not been completed
                    {
                        stepLogger.AddItem(record, "Step 3 - Verifications were not complete based on SIEBEL call notes.", "FAIL - fraudelant request");
                    }
                }
            }
            _jobstatus.Status = "Step 3 completed";
            #endregion

            #region Step 3a: Check Eppic Record Against Seibel CallNotes OTP Check only if last step is set to pass
            // TBD.  Here I would like to add the additional JSON data from the SK logic to the data we are logging to the Collection
            // Need to look into how to add optional JSON data to the structure
            // In this step you can leverage basically that same type of logic that is in Step 3.
            _jobstatus.Status = "Processing Completed";
            #endregion
            // return "Steps have ran";
            // write the file to disk
            await stepLogger.WriteLogCollectionToDisk(_stepslogfile.FileName ?? "", Constants.UseLocalFiles);
        }

        #region Example JSON response
        /*
            {
                "SiebelLookupResult": {
                    "StepName": "Siebel Lookup",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "AddressCheckResult": {
                    "StepName": "Address Check",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "GoogleSearchResult": {
                    "StepName": "Google Search",
                    "StatusCode": "PASS",
                    "Message": "Success"
                },
                "MasterStatusCode":"PASS"
            }

        */
        #endregion

        #region GoogleSearch Not bing used
        //private bool PerformGoogleSearch(string address)
        //{
        //    // Implement Google Search API call to verify address
        //    // Replace with actual implementation
        //    using (var client = new HttpClient())
        //    {
        //        // var address = "1600 Amphitheatre Parkway, Mountain View, CA"; // Replace with actual address
        //        var apiKey = "YOUR_GOOGLE_API_KEY"; // Replace with actual API key
        //        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={address}&key={apiKey}";
        //        var response = client.GetAsync(url).Result;
        //        var content = response.Content.ReadAsStringAsync().Result;
        //        var jsonData = JsonDocument.Parse(content);
        //        var results = jsonData.RootElement.GetProperty("results");
        //        foreach (var result in results.EnumerateObject())
        //        {
        //            var types = result.Value.GetProperty("types");
        //            if (types.EnumerateArray().Any(t => t.GetString() == "hospital" || t.GetString() == "health"))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        #endregion
    }
}

