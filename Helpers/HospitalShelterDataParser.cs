using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using api_process_runner_api.Models;
using FileHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace api_process_runner_api.Helpers
{
    internal class HospitalShelterDataParser
    {
        private int _countofRecords = 0;
        public List<HospitalShelterRecords>? hospitalRecordsList;

        public void LoadData(StreamReader reader)
        {
            var engineHospitals = new FileHelperEngine<HospitalShelterRecords>();
            var recordsSiebel = engineHospitals.ReadStream(reader);
            this.hospitalRecordsList = recordsSiebel.ToList();
        }

        public int CountOfRecords
        {
            get
            {
                return _countofRecords;
            }
        }

        public List<HospitalShelterRecords> ParseCsv()
        {
            return hospitalRecordsList ?? new List<HospitalShelterRecords>();
        }
        public void PrintHospitalRecords(List<HospitalShelterRecords> recordsEppic)  // Used for debugging purposes
        {
            var count = 0;
            foreach (var recordEppic in recordsEppic)
            {
                count++;
                Console.WriteLine($@"Record# {count} AddressLine1: {recordEppic.AddressLine1} AddressLine2: {recordEppic.AddressLine2} City: {recordEppic.City} State: {recordEppic.State} ");
            }
        }

        public HospitalShelterRecords? FindHospitalByAddress1(string address1, List<HospitalShelterRecords> hospitalrecords)  
        {
            return hospitalrecords.FirstOrDefault(record => record.AddressLine1 == address1);
        }

        public HospitalShelterRecords? FindHospitalByFullAddress(string address1, string address2, string city, string state, List<HospitalShelterRecords> hospitalrecords)
        {
            //return hospitalrecords.FirstOrDefault(record => record.Phone_Number == phonenumber);
            return hospitalrecords.FirstOrDefault(record =>
                    record.AddressLine1 == address1 &&
                    record.AddressLine2 == address2 &&
                    record.City == city &&
                    record.State == state);
        }

    }
}

