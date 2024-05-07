using api_process_runner_api.Models.Reporting;
using api_process_runner_api.Util;
using Azure.Storage.Blobs;

namespace api_process_runner_api.Helpers.Reporting
{
    public class VerificationConclusionManager
    {
        private List<VerificationConclusion> _verificationConclusionResults = new List<VerificationConclusion>();
        public List<VerificationConclusion> VerificationConclusionResults { get { return _verificationConclusionResults; } }
        private bool _headercreated = false;

        // Method to add or update an item
        public void AddOrUpdateVerificationConclusion(VerificationConclusion newItem)
        {
            _verificationConclusionResults.Add(newItem);
            ////var existingItem = _verificationConclusionResults.FirstOrDefault(item => item.PersonID == newItem.PersonID);

            ////if (existingItem == null)
            ////{
            ////    // Add new item
            ////    _verificationConclusionResults.Add(newItem);
            ////}
            ////else
            ////{
            //// Update existing item
            //existingItem.ActivityRelatedTo = newItem.ActivityRelatedTo;
            //    existingItem.FormOfAuthentication = newItem.FormOfAuthentication;
            //    existingItem.PhoneNumber = newItem.PhoneNumber;
            //    existingItem.VerificationsCompleted = newItem.VerificationsCompleted;
            ////}
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
            string csvFilePath = $@"{Constants.LocalFilePath}\CSVResults\VerificationConclusionResults_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}.csv";

            using (StreamWriter writer = new StreamWriter(csvFilePath))
            {
                if (!_headercreated)
                {
                    writer.WriteLine($"PersonID,ActivityRelatedTo,FormOfAuthentication,PhoneNumber,VerificationsCompleted");
                    _headercreated = true;
                }
                foreach (var item in _verificationConclusionResults)
                {
                    writer.WriteLine($"{item.PersonID},{item.ActivityRelatedTo},{item.FormOfAuthentication},{item.PhoneNumber},{item.VerificationsCompleted}");
                }
            }
        }

        private void WriteToAzureBlob()
        {
            var _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");

            string connectionString = _blobConnection;
            string containerName = "csvresults";

            BlobServiceClient blobServiceClient = new BlobServiceClient(_blobConnection);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists();

            string fileName = $"VerificationConclusionResults_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}.csv";

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    if (!_headercreated)
                    {
                        writer.WriteLine($"PersonID,ActivityRelatedTo,FormOfAuthentication,PhoneNumber,VerificationsCompleted");
                        _headercreated = true;
                    }
                    foreach (var item in _verificationConclusionResults)
                    {
                        writer.WriteLine($"{item.PersonID},{item.ActivityRelatedTo},{item.FormOfAuthentication},{item.PhoneNumber},{item.VerificationsCompleted}");
                    }

                    writer.Flush();
                    memoryStream.Position = 0;
                    blobClient.Upload(memoryStream);
                }
            }
        }
    }
}
