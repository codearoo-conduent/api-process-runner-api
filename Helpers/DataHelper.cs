using api_process_runner_api.Models;
using api_process_runner_api.Util;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace api_process_runner_api.Helpers
{
    static class Globals
    {
        public static List<HospitalShelterRecords>? hospitalRecords;
        public static List<SiebelRecords>? siebelRecords;
        public static List<EppicRecords>? eppicRecords;
        public static List<GiactRecords>? giactRecords;
        public static IEnumerable<EppicRecords>? inputEppicRecordsInHospitalDB;
        public static IEnumerable<EppicRecords>? inputEppicRecordsNotInHospitalDB;
        public static IEnumerable<EppicRecords>? inputEppicRecordsInGiactDB;
        public static IEnumerable<EppicRecords>? inputEppicRecordsNotInGiactDB;
    }

    internal class DataHelper
    {
        private BlobHelper _blobHelper;
        private Kernel _kernel;
        private bool _usingLocalFiles;
        private UploadedFilesRequest _uploadedFilesRequest;
        private SiebelDataParser _siebelDataParser;
        private EppicDataParser _eppicDataParser;
        private GiactDataParser _giactDataParser;
        private HospitalShelterDataParser _hospitalShelterDataParser;
        private List<HospitalShelterRecords>? _hospitaldataRecords;
        private List<EppicRecords>? _eppicdataRecords;
        private List<SiebelRecords>? _siebeldataRecords;
        private List<GiactRecords>? _giactdataRecords;

        public SiebelDataParser SiebelDataParser        {
            get { return _siebelDataParser; }
        }
        public EppicDataParser EppicDataParser
        {
            get { return _eppicDataParser; }
        }
        public GiactDataParser GiactDataParser
        {
            get { return _giactDataParser; }
        }
        public HospitalShelterDataParser HospitalShelterDataParser
        {
            get { return _hospitalShelterDataParser; }
        }

        public List<HospitalShelterRecords>? HospitalDataRecords
        {
            get { return _hospitaldataRecords; }
        }
        public List<EppicRecords>? EppicDataRecords
        {
            get { return _eppicdataRecords; }
        }
        public List<SiebelRecords>? SiebelDataRecords
        {
            get { return _siebeldataRecords; }
        }
        public List<GiactRecords>? GiactDataRecords
        {
            get { return _giactdataRecords; }
        }

        public DataHelper(UploadedFilesRequest uploadedFilesRequest,string blobconnectionstring, bool usingLocalFiles, Kernel kernel) 
        {
            _usingLocalFiles = usingLocalFiles; 
            _kernel = kernel;
            _uploadedFilesRequest = uploadedFilesRequest;
            _blobHelper = new BlobHelper()
            {
                Container = "eppic",
                ConnectionString = blobconnectionstring
            };
            _siebelDataParser = new SiebelDataParser();
            _eppicDataParser = new EppicDataParser();
            _giactDataParser = new GiactDataParser();
            _hospitalShelterDataParser = new HospitalShelterDataParser();
        }


        public async Task<string> Intialize()
        {
            var result = "Blank";
            if (_usingLocalFiles)
            {
                try
                {
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Siebel\{_uploadedFilesRequest.SiebelFilename}"))
                    {
                        _siebelDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Eppic\{_uploadedFilesRequest.EppicFilename}"))
                    {
                        _eppicDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Giact\{_uploadedFilesRequest.GiactFilename}"))
                    {
                        _giactDataParser.LoadData(stream);
                    }
                    using (StreamReader stream = File.OpenText($@"{Constants.LocalFilePath}\Hospitals\{_uploadedFilesRequest.AddressFilename}"))
                    {
                        _hospitalShelterDataParser.LoadData(stream);
                    }
                    _hospitaldataRecords = _hospitalShelterDataParser.ParseCsv();
                    _eppicdataRecords = _eppicDataParser.ParseCsv();
                    _siebeldataRecords = _siebelDataParser.ParseCsv();
                    _giactdataRecords = _giactDataParser.ParseCsv();
                    Globals.hospitalRecords = _hospitaldataRecords;
                    Globals.eppicRecords = _eppicdataRecords;
                    Globals.siebelRecords = _siebeldataRecords;
                    Globals.giactRecords = _giactdataRecords;
                    Step1_BuildEppicListWithMatchesInHospital();
                    Step1_BuildEppicListWithoutInAgainstHospital();
                    Step2_BuildEppicListWithMatchesInGiact();
                    Step2_BuildEppicListWithMatchesNotInGiact();
                    result = "Success";
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e.ToString());
                    return result = "Failed to load stream";
                }

            }
            else // Using Azure Storage
            {
                try
                {
                    // Use below to load from Azure Blob Storage note how this uses the Blob Helper
                    _blobHelper.Container = "hospitals";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.AddressFilename ?? String.Empty))
                    {
                        _hospitalShelterDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "siebel";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.SiebelFilename ?? String.Empty))
                    {
                        _siebelDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "eppic";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.EppicFilename ?? String.Empty))
                    {
                        _eppicDataParser.LoadData(stream);
                    }
                    _blobHelper.Container = "giact";
                    using (StreamReader stream = await _blobHelper.GetStreamReaderFromBlob(_uploadedFilesRequest.GiactFilename ?? String.Empty))
                    {
                        _giactDataParser.LoadData(stream);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return result = "Failed to load stream";
                }
            }
            return result;
        }

        public void Step1_BuildEppicListWithMatchesInHospital()
        {
            // Builds a List of Eppic records that have a match in Hospital DB
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");
            if (Globals.hospitalRecords != null)
            {
                Globals.inputEppicRecordsInHospitalDB =
                    from e in Globals.eppicRecords
                    join a in Globals.hospitalRecords
                        on new { e.AddressLine1, e.AddressLine2, e.City, e.State }
                        equals new { a?.AddressLine1, a?.AddressLine2, a?.City, a?.State }
                    select e;
                Console.WriteLine($"Recs that match in the address DB: {Globals.inputEppicRecordsInHospitalDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }

        public void Step1_BuildEppicListWithoutInAgainstHospital()
        {
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");

            // Check if Globals.hospitalRecords is not null
            if (Globals.hospitalRecords != null && Globals.eppicRecords != null)
            {
                // Find Eppic records that do not match in the hospitalRecords
                Globals.inputEppicRecordsNotInHospitalDB =
                    Globals.eppicRecords.Except(
                        from e in Globals.eppicRecords
                        join a in Globals.hospitalRecords
                            on new { e.AddressLine1, e.AddressLine2, e.City, e.State }
                            equals new { a?.AddressLine1, a?.AddressLine2, a?.City, a?.State }
                        select e);

                Console.WriteLine($"Recs not matching in the address DB: {Globals.inputEppicRecordsNotInHospitalDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }

        public void Step2_BuildEppicListWithMatchesInGiact()
        {
            // Builds a List of Eppic records that have a match in Hospital DB
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");
            if (Globals.giactRecords != null && Globals.eppicRecords != null)
            {
                Globals.inputEppicRecordsInGiactDB =
                    from e in Globals.eppicRecords
                    join a in Globals.giactRecords
                        on new { e.AddressLine1, e.City, e.State, e.ZipCode }
                        equals new { a?.AddressLine1, a?.City, a?.State, a?.ZipCode }
                    select e;
                Console.WriteLine($"Recs that match in the Giact DB: {Globals.inputEppicRecordsInGiactDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }

        public void Step2_BuildEppicListWithMatchesNotInGiact()
        {
            // Builds a List of Eppic records that have a match in Hospital DB
            Console.WriteLine($"Total Eppic records: {Globals.eppicRecords?.Count}");
            if (Globals.giactRecords != null && Globals.eppicRecords != null)
            {
                // TBD Needs review as Giact only has AddressLine1 not AddressLine2 which Eppic has.  Also, not sure with the Addressline19 thing comes into play
                Globals.inputEppicRecordsNotInGiactDB =
                   Globals.eppicRecords.Except(
                       from e in Globals.eppicRecords
                       join a in Globals.giactRecords
                           on new { e.AddressLine1, e.City, e.State, e.ZipCode }
                           equals new { a?.AddressLine1, a?.City, a?.State, a?.ZipCode }
                       select e);
                Console.WriteLine($"Recs that have not match in the Giact DB: {Globals.inputEppicRecordsNotInGiactDB.Count()}");
            }
            else
            {
                // Handle the case when Globals.hospitalRecords is null
                // For example, log a warning or provide a default value
            }
        }

        public void ClearCollections()
        {
            Globals.eppicRecords?.Clear();
            Globals.hospitalRecords?.Clear();
            Globals.siebelRecords?.Clear();
            Globals.giactRecords?.Clear();
            if (Globals.inputEppicRecordsNotInHospitalDB != null && Globals.inputEppicRecordsInHospitalDB != null)
                {
                    List<EppicRecords> eppicListNotIn = Globals.inputEppicRecordsNotInHospitalDB.ToList();
                    eppicListNotIn.Clear();
                    List<EppicRecords> eppicListIn = Globals.inputEppicRecordsInHospitalDB.ToList();
                    eppicListIn.Clear();
                }
            HospitalDataRecords?.Clear();
            EppicDataRecords?.Clear();
            SiebelDataRecords?.Clear();
            GiactDataRecords?.Clear();
        }

        public async Task<string> RunTestsOnData(DataHelper dataHelper)
        {
            try
            {
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
                return "All Tests have ran";
            }
            catch (Exception ex)
            {
                var errormsg = $"Issue running the tests. Exception: {ex.ToString()}";
                Console.WriteLine(errormsg);
                return errormsg;
            }
        }
    }
}
