using api_process_runner_api.Models;
using api_process_runner_api.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public static IEnumerable<EppicRecords>? inputEppicRecordsMatchedGiactAddress;
        public static IEnumerable<EppicRecords>? inputEppicRecordsNotMatchedGiactAddress;
        public static IEnumerable<EppicRecords>? inputEppicRecordsMatchingGiactPhoneNumber;
    }

    internal class DataHelper
    {
        private BlobHelper _blobHelper;
        private int _countofRecords = 0;
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

        public DataHelper(UploadedFilesRequest uploadedFilesRequest,string blobconnectionstring, bool usingLocalFiles) 
        {
            _usingLocalFiles = usingLocalFiles; 
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
                }
                catch (Exception e) 
                { 
                    Console.WriteLine(e.ToString());
                    throw;
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
                    throw;
                }
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
            Step2_BuildEppicListMatchingGiactAddresses();
            Step2_BuildEppicListNotMatchingGiactAddresses();
            Step3_BuildEppicListMatchingGiactPhoneNumber();
            result = "Success";

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

        public void Step2_BuildEppicListMatchingGiactAddresses()
        {
            Console.WriteLine($"Total Eppic records: {Globals.inputEppicRecordsNotInHospitalDB?.Count()}");

            if (Globals.inputEppicRecordsNotInHospitalDB != null && Globals.giactRecords != null)
            {

                Globals.inputEppicRecordsMatchedGiactAddress = (
                    from e in Globals.inputEppicRecordsNotInHospitalDB
                    join g in Globals.giactRecords on e.PersonID equals g.UniqueID
                    where string.Compare(e.AddressLine1?.Trim(), g.AddressCurrent?.Trim(), ignoreCase: true) == 0
                    where string.Compare(e.City?.Trim(), g.CityCurrent?.Trim(), ignoreCase: true) == 0
                    where string.Compare(e.ZipCode?.Trim(), g.ZipCodeCurrent?.Trim(), ignoreCase: true) == 0
                    select e).Distinct();

                Globals.inputEppicRecordsMatchedGiactAddress = Globals.inputEppicRecordsMatchedGiactAddress.Concat((
                    from e in Globals.inputEppicRecordsNotInHospitalDB
                    join g in Globals.giactRecords on e.PersonID equals g.UniqueID
                    where string.Compare(e.AddressLine1, g.AddressPast, ignoreCase: true) == 0
                    where string.Compare(e.City, g.CityPast, ignoreCase: true) == 0
                    where string.Compare(e.ZipCode, g.ZipCodePast, ignoreCase: true) == 0
                    select e).Distinct()
                    );

                Globals.inputEppicRecordsMatchedGiactAddress = Globals.inputEppicRecordsMatchedGiactAddress.Distinct();

                Console.WriteLine($"Recs matching a current or past address in Giact: { Globals.inputEppicRecordsMatchedGiactAddress.Count() } ");
            }
        }

        public void Step2_BuildEppicListNotMatchingGiactAddresses()
        {
            Console.WriteLine($"Total Eppic records: {Globals.inputEppicRecordsNotInHospitalDB?.Count()}");

            if (Globals.inputEppicRecordsNotInHospitalDB != null && Globals.inputEppicRecordsMatchedGiactAddress != null)
            {
                Globals.inputEppicRecordsNotMatchedGiactAddress =
                    Globals.inputEppicRecordsNotInHospitalDB.Except(Globals.inputEppicRecordsMatchedGiactAddress);

                Console.WriteLine($"Recs not matching in Giact addresses: {Globals.inputEppicRecordsNotMatchedGiactAddress.Count()} ");
            }
        }

        public void Step3_BuildEppicListMatchingGiactPhoneNumber()
        {
            Console.WriteLine($"Total Eppic records: {Globals.inputEppicRecordsNotInHospitalDB?.Count()}");

            if (Globals.inputEppicRecordsNotInHospitalDB != null && Globals.giactRecords != null)
            {

                Globals.inputEppicRecordsMatchingGiactPhoneNumber = (
                    from e in Globals.inputEppicRecordsNotInHospitalDB
                    join g in Globals.giactRecords on e.PersonID equals g.UniqueID
                    where string.Compare(e.Phone_Number?.Trim(), g.PhoneNumber?.Trim(), ignoreCase: true) == 0
                    select e).Distinct();

                Globals.inputEppicRecordsMatchingGiactPhoneNumber = Globals.inputEppicRecordsMatchingGiactPhoneNumber.Distinct();

                Console.WriteLine($"Recs matching a current or past phone number in Giact: {Globals.inputEppicRecordsMatchingGiactPhoneNumber.Count()} ");
            }
        }

        public static void LoadDataFiles()
        {

        }
    }
}
