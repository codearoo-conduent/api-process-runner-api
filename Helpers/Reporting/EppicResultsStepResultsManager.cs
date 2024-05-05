using api_process_runner_api.Models.Reporting;
using api_process_runner_api.Util;
using System.Collections;
using System.Reflection.Metadata;
using Azure.Storage.Blobs;
namespace api_process_runner_api.Helpers.Reporting
{
    public class EppicStepResultsManager
    {
        private List<EppicStepResults> _eppicStepsResults = new List<EppicStepResults>();

        public List<EppicStepResults> EppicStepResults { get { return _eppicStepsResults; } }

        // Method to add a new item
        public void AddOrUpdateEppicStepResult(EppicStepResults newItem)
        {
            var existingItem = _eppicStepsResults.FirstOrDefault(item => item.LogID == newItem.LogID);

            if (existingItem == null)
            {
                // Add new item
                _eppicStepsResults.Add(newItem);
            }
            else
            {
                // Update existing item
                existingItem.LogDate = newItem.LogDate;
                existingItem.PersonID = newItem.PersonID;
                existingItem.PhoneNumber = newItem.PhoneNumber;
                existingItem.AddressLine1 = newItem.AddressLine1;
                existingItem.AddressLine2 = newItem.AddressLine2;
                existingItem.City = newItem.City;
                existingItem.State = newItem.State;
                existingItem.Zip = newItem.Zip;
                existingItem.Step1HospitalMatch = newItem.Step1HospitalMatch;
                existingItem.Step2GiactMatch = newItem.Step2GiactMatch;
                existingItem.Step3PassedVerificationCheck = newItem.Step3PassedVerificationCheck;
                existingItem.Step3aPassedOTPPhoneGiact = newItem.Step3aPassedOTPPhoneGiact;
                existingItem.LastStepCompleted = newItem.LastStepCompleted;
            }
        }

        public void WriteToCsv(bool useLocalFiles)
        {
            if (useLocalFiles)
            {
                WriteToLocalCsv();
            }
            else
            {
                WriteToAzureBlob();
            }
        }

        private void WriteToLocalCsv()
        {
            string csvFilePath = $@"{Constants.LocalFilePath}\CSVResults\EppicStepResults.csv";

            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                foreach (var item in _eppicStepsResults)
                {
                    writer.WriteLine($"{item.LogID},{item.LogDate},{item.PersonID},{item.PhoneNumber},{item.AddressLine1},{item.AddressLine2},{item.City},{item.State},{item.Zip},{item.Step1HospitalMatch},{item.Step2GiactMatch},{item.Step3PassedVerificationCheck},{item.Step3aPassedOTPPhoneGiact},{item.LastStepCompleted},{item.Status}");
                }
            }
        }
        private void WriteToAzureBlob()
        {
            var _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");
            var _blobHelper = new BlobHelper()
            {
                Container = "CSVResults",
                ConnectionString = _blobConnection
            };

            string connectionString = _blobConnection;
            string containerName = "CVSResults";

            BlobServiceClient blobServiceClient = new BlobServiceClient(_blobConnection);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            string fileName = $"EppicStepsResults_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}.csv";

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    foreach (var item in _eppicStepsResults)
                    {
                        writer.WriteLine($"{item.LogID},{item.LogDate},{item.PersonID},{item.PhoneNumber},{item.AddressLine1},{item.AddressLine2},{item.City},{item.State},{item.Zip},{item.Step1HospitalMatch},{item.Step2GiactMatch},{item.Step3PassedVerificationCheck},{item.Step3aPassedOTPPhoneGiact},{item.LastStepCompleted},{item.Status}");
                    }

                    writer.Flush();
                    memoryStream.Position = 0;
                    blobClient.Upload(memoryStream);
                }
            }
        }
    }

}
