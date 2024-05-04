using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using api_process_runner_api.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace api_process_runner_api.Helpers
{
    public class GiactDataParser
    {
        private int _countofRecords = 0;
        public List<GiactRecords>? giactRecordsList;

        public void LoadData(StreamReader reader)
        {
            var engineGiact = new FileHelperEngine<GiactRecords>();
            var recordsGiact = engineGiact.ReadStream(reader);
            this.giactRecordsList = recordsGiact.ToList();
        }
        public int CountOfRecords
        {
            get
            {
                return _countofRecords;
            }
        }

        public List<GiactRecords> ParseCsv()
        {
            return giactRecordsList ?? new List<GiactRecords>();
        }
        public void PrintGiactRecords(List<GiactRecords> recordsGiact)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordGiact in recordsGiact)
            {
                count++;
                Console.WriteLine($@"Record# {count} UniqueID: {recordGiact.UniqueID} AddressLine1: {recordGiact.AddressLine1} City: {recordGiact.City} State: {recordGiact.State} ZipCode: {recordGiact.ZipCode} AddressCurrentPast: {recordGiact.AddressCurrentPast} CityCurrentPast: {recordGiact.CityCurrentPast} Status: {recordGiact.Status}");
            }
        }

        public GiactRecords? FindGiactUniqueID(string uniqueid, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.UniqueID == uniqueid);
        }

        public List<GiactRecords>? FindGiactRecordsByUniqueID(string uniqueId)
        {
            return giactRecordsList.FindAll(record => record.UniqueID == uniqueId);
        }

        public GiactRecords? FindGiactByAddressLine1(string address, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressLine1 == address);
        }

        public GiactRecords? FindGiactByAddressCurrentPast(string addresscurrentpast, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressCurrentPast == addresscurrentpast);
        }

        public GiactRecords? FindGiactByFullAddress(string addressline1, string city, string state, string zipcode, List<GiactRecords> giactrecords)
        { // Eppic Address1, Address2, CITY, STATE, ZIP
           return giactrecords.FirstOrDefault(record =>
                    record.AddressLine1 == addressline1 &&
                    record.City == city &&
                    record.State == state &&
                    record.ZipCode == zipcode);
        }

    }
}

