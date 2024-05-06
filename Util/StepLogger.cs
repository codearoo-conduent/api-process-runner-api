using Azure.Storage.Blobs;
using FileHelpers;
using System.Diagnostics;
using System.Security.Cryptography.Xml;
using System.Text;
using System;
using System.Text.Json;


namespace api_process_runner_api.Util;

public class ProcessStep
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public List<EppicRecord>? EppicRecords { get; set; }
}

public class EppicRecord
{
    public string? PersonID { get; set; }
    public string? Phone_Number { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

public class StepLogger
{ 
    public List<ProcessStep> processSteps = new List<ProcessStep>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="record"></param>
    /// <param name="stepTitle">Include the Step number in this title</param>
    /// <param name="status"></param>
    public void AddItem(EppicRecord record, string stepTitle, string status)
    {
        var step = (from a in this.processSteps
                    where a.Title == stepTitle
                    where a.Status == status
                    select a).FirstOrDefault();
        if (step == null)
        {
            step = new ProcessStep() { Title = stepTitle, Status = status, EppicRecords = [] };
            this.processSteps.Add(step);
        }
        step.EppicRecords?.Add(record);
    }

    /* expected output
{
  "ProcessStep": {
    "Title": "Eppic records that failed address lookup",
    "Status": "recorded",
    "EppicRecords": [
      {
        "PersonID": "5094334",
        "Phone_Number": "2512271296",
        "AddressLine1": "705 qfGzfLlf fVE",
        "AddressLine2": " ",
        "City": "EVERGREEN",
        "State": "AL",
        "ZipCode": "36401"
      },
      {
        "PersonID": "6359555",
        "Phone_Number": "7866552347",
        "AddressLine1": "400 pEzdqfL fVE z",
        "AddressLine2": "APT 209",
        "City": "CHISHOLM",
        "State": "MN",
        "ZipCode": "55719"
      }
    ]
  }
}     
     */

    public void PrintData()
    {
        var json = JsonSerializer.Serialize(this.processSteps,
            new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });

        Console.WriteLine(json);
    }


    //public async Task<bool> WriteToBlobAsync(Stream fileStream,string blobName)
    //{
    //    // TBD: Try Catch 
    //    var blobServiceClient = new BlobServiceClient(ConnectionString); 
    //    var containerClient = blobServiceClient.GetBlobContainerClient(Container);  
    //    await containerClient.CreateIfNotExistsAsync();
    //    var blobClient = containerClient.GetBlobClient(blobName);
    //    await blobClient.UploadAsync(fileStream, true);
    //    return true;

    //}

    public static class TestStepLogger
    {
        public static void Test()
        {
            var logger = new StepLogger();

            var e1 = new EppicRecord()
            {
                PersonID = "1"
            };
            var e2 = new EppicRecord()
            {
                PersonID = "2"
            };

            logger.AddItem(e1, "step 1", "passed");
            logger.AddItem(e1, "step 2", "passed");

            logger.AddItem(e2, "step 1", "passed");
            logger.AddItem(e2, "step 2", "failed");

            logger.PrintData();
        }
    }

}