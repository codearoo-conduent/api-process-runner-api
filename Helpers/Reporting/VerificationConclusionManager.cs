using api_process_runner_api.Models.Reporting;
using api_process_runner_api.Util;
using Azure.Storage.Blobs;
using FileHelpers;

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
            var existingItem = _verificationConclusionResults.FirstOrDefault(item => item.PersonID == newItem.PersonID);

            if (existingItem == null)
            {
                // Add new item
                _verificationConclusionResults.Add(newItem);
            }
            else
            {
                existingItem.ActivityRelatedTo = newItem.ActivityRelatedTo;
                existingItem.FormOfAuthentication = newItem.FormOfAuthentication;
                existingItem.PhoneNumber = newItem.PhoneNumber;
                existingItem.VerificationsCompleted = newItem.VerificationsCompleted;
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
            string csvFilePath = $@"{Constants.LocalFilePath}\CSVResults\VerificationConclusionResults_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString().Substring(0, 8)}.csv";

            var fileEngine = new FileHelperEngine<VerificationConclusion>();
            fileEngine.HeaderText = fileEngine.GetFileHeader();
            fileEngine.WriteFile(csvFilePath, _verificationConclusionResults);

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

            var fileEngine = new FileHelperEngine<VerificationConclusion>();
            fileEngine.HeaderText = fileEngine.GetFileHeader();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream))
                {
                    fileEngine.WriteStream(writer, _verificationConclusionResults);

                    writer.Flush();
                    memoryStream.Position = 0;
                    blobClient.Upload(memoryStream);
                }
            }
        }
    }
}
