using api_process_runner_api.Helpers;
using api_process_runner_api.Models;
using Azure.Storage.Blobs;
using FileHelpers;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Text.Json;


namespace api_process_runner_api.Util;

public class ProcessStep
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public List<EppicRecords>? EppicRecords { get; set; }
}

public class StepLogger
{ 
    public List<ProcessStep> processSteps = new List<ProcessStep>();

    public void TestAddItems()
    {
        List<EppicRecords> eppicrecords = new List<EppicRecords>();
        for (int i = 0; i < 10; i++)
        {
            eppicrecords.Add(new EppicRecords()
            {
                PersonID = i.ToString(),
                Phone_Number = $"7045550{i.ToString()}",
                AddressLine1 = "Test Data",
                AddressLine2 = "Test Data",
                City = "Charlotte",
                ZipCode = "28808",
                State = "NC"
            });
        }
        foreach (var record in eppicrecords)
        {
            AddItem(record, "Step 1 - Eppic Records Found in Hospitals", "PASS");
        }

    }

    public void PrintItems()
    {
        // Create JsonSerializerOptions with indented formatting
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        foreach (var record in processSteps)
        {
            string json = JsonSerializer.Serialize(record, options);
            Console.WriteLine(json);
        }
    }

    public async Task<string> WriteLogCollectionToDisk(string logfilename, bool usingLocalFiles)
    {
        var _blobConnection = Helper.GetEnvironmentVariable("BlobConnection");
        BlobHelper blobHelper = new BlobHelper()
        {
            ConnectionString = _blobConnection,
            Container = "Logs"
        };
        var result = "";
        if (usingLocalFiles)
        {   // If set to use local files this branch will be executed
            try
            {
                // Create JsonSerializerOptions with indented formatting
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                // Write byte array to a local file
                var filePath = $@"{Constants.LocalFilePath}\Logs\{logfilename}";

                string json = JsonSerializer.Serialize(processSteps, options);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                File.WriteAllBytes(filePath, byteArray);
                return result = $@"Wrote the logsteps to: {filePath}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return result = $@"Failed to write file to Local filesystem";
            }

        }
        else // Using Azure Storage this branch will be executed
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(processSteps, options);
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                // Upload Byte array to the blob
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    if (blobHelper != null)
                    {
                        await blobHelper.WriteToBlobAsync(stream, logfilename);
                    }
                }
                return result = $@"Wrote the logsteps to BlobContiner: {blobHelper?.Container} Blob: {logfilename}";

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return result = $@"Failed to write LogSteps to BlobContainer {blobHelper?.Container}";
            }
        }
    }

    public void AddItem(EppicRecords record, string stepTitle, string status)
    {
        var step = processSteps.FirstOrDefault(a => a.Title == stepTitle && a.Status == status);
        if (step == null)
        {
            step = new ProcessStep() { Title = stepTitle, Status = status, EppicRecords = new List<EppicRecords>() };
            processSteps.Add(step);
        }
        step.EppicRecords?.Add(record);
    }





}