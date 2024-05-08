using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using api_process_runner_api.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace api_process_runner_api.Helpers.Parsers
{
    public class SiebelDataParser
    {
        private List<SiebelCallNotes>? _siebelCallNotes;
        public List<SiebelRecords>? siebelRecordsList;
        private int _countofCallNotes = 0;

        public void LoadData(StreamReader reader)
        {
            /* Examples of calling LoadData
            var parser = new SiebelDataParser();
            //using (StreamReader stream = File.OpenText(@"Data\Siebel\scrubbedSampleSiebel.csv"))
            using (StreamReader stream = await blobHelper.GetStreamReaderFromBlob("scrubbedSampleSiebel.csv"))
            {
                parser.LoadData(stream);
            }

            var output = parser.ParseCsv();
            parser.PrintSiebelRecords(output);
            */
            var engineSiebel = new FileHelperEngine<SiebelRecords>();
            var recordsSiebel = engineSiebel.ReadStream(reader);
            siebelRecordsList = recordsSiebel.ToList();
        }
        public int CountOfCallNotes
        {
            get
            {
                return _countofCallNotes;
            }
        }
        public List<SiebelCallNotes> CallNotes
        {
            get
            {
                if (_siebelCallNotes == null)
                {
                    _siebelCallNotes = new List<SiebelCallNotes>();
                }

                return _siebelCallNotes;
            }
        }
        public List<SiebelRecords> ParseCsv()
        {

            // Now let's filter all the records that actually have ActivityDescriptions and copy those into it's own list for use later
            _siebelCallNotes = siebelRecordsList?
                 .Where(record => record.ActivityDescription != "")
                 .Select(record => new SiebelCallNotes { PersonID = record.PersonID, CallNotes = record.ActivityDescription, ActivityCreatedDate = record.ActivityCreatedDate })
                 .ToList();
            // store the count of items that actually have CallNotes
            _countofCallNotes = _siebelCallNotes?.Count ?? 0;
            return siebelRecordsList ?? new List<SiebelRecords>();
        }
        public void PrintSiebelRecords(List<SiebelRecords> recordsSiebel)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordSiebel in recordsSiebel)
            {
                count++;
                Console.WriteLine($@"Record# {count} PersonID: {recordSiebel.PersonID} ActivityCreatedDate: {recordSiebel.ActivityCreatedDate} ");
                if (recordSiebel.ActivityDescription != "")
                {
                    Console.WriteLine("\n\n\n");
                    Console.WriteLine("Activity Description has data!!!");
                    Console.WriteLine("***************************");
                    Console.WriteLine(recordSiebel.ActivityDescription);
                    Console.WriteLine("***************************\n\n");
                }
            }
        }

        public void PrintSiebelCallNoteRecords(List<SiebelCallNotes> recordsSiebel)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordSiebel in recordsSiebel)
            {
                count++;
                Console.WriteLine($@"Record# {count} PersonID: {recordSiebel.PersonID} CallNotes: {recordSiebel.CallNotes}");
            }
        }

        public SiebelCallNotes? FindSiebelCallNoteByPersonID(string personID)  // returns only one callnote note sure if this is valid as it appears a personID can have multuple callnotes
        {
            // this is one we need clarification on.
            if (_siebelCallNotes == null)
                return null; // If _siebelCallNotes is null, return null

            return _siebelCallNotes.FirstOrDefault(note => note.PersonID == personID);
        }

        public List<SiebelCallNotes> FindAllSiebelCallNotesByPersonIDLastFirst(string personID)
        {
            if (_siebelCallNotes == null)
                return new List<SiebelCallNotes>(); // If _siebelCallNotes is null, return an empty list

            return _siebelCallNotes.Where(note => note.PersonID == personID)
                .OrderByDescending(note => note.ActivityCreatedDate)
                .ToList();
        }

    }
}

