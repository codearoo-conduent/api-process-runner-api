using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using api_process_runner_api.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace api_process_runner_api.Helpers
{
    public class EppicDataParser
    {
        private int _countofRecords = 0;
        public List<EppicRecords>? eppicRecordsList;

        public void LoadData(StreamReader reader)
        {
            var engineEppic = new FileHelperEngine<EppicRecords>();
            var recordsEppic = engineEppic.ReadStream(reader);
            this.eppicRecordsList = recordsEppic.ToList();
        }

        public int CountOfRecords
        {
            get
            {
                return _countofRecords;
            }
        }

        public List<EppicRecords> ParseCsv()
        {
            return eppicRecordsList ?? new List<EppicRecords>();
        }

        public void PrintEppicRecords(List<EppicRecords> recordsEppic)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordEppic in recordsEppic)
            {
                count++;
                Console.WriteLine($@"Record# {count} PersonID: {recordEppic.PersonID} Phone_Number: {recordEppic.Phone_Number} AddressLine1: {recordEppic.AddressLine1} AddressLine2: {recordEppic.AddressLine2} City: {recordEppic.City} State: {recordEppic.State} Zip: {recordEppic.ZipCode}");
            }
        }

        public EppicRecords? FindEppicPersonID(string personID, List<EppicRecords> eppicrecords)  
        {
            return eppicrecords.FirstOrDefault(record => record.PersonID == personID);
        }

        public EppicRecords? FindEppicByPhone(string phonenumber, List<EppicRecords> eppicrecords)
        {
            return eppicrecords.FirstOrDefault(record => record.Phone_Number == phonenumber);
        }

        public EppicRecords? FindEppicByAddress(string address, List<EppicRecords> eppicrecords)
        {
            return eppicrecords.FirstOrDefault(record => record.AddressLine1 == address);
        }

    }
}

