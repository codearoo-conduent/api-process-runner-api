using api_process_runner_api.Models;
using Azure.Storage.Blobs;
using FileHelpers;
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