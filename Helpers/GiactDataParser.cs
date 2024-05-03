﻿using System;
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
                Console.WriteLine($@"Record# {count} UniqueID: {recordGiact.UniqueID} AddressLine1: {recordGiact.AddressCurrent} City: {recordGiact.CityCurrent} State: {recordGiact.StateCurrent} ZipCode: {recordGiact.ZipCodeCurrent} AddressCurrentPast: {recordGiact.AddressPast} CityCurrentPast: {recordGiact.CityPast} Status: {recordGiact.Status}");
            }
        }

        public GiactRecords? FindGiactUniqueID(string uniqueid, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.UniqueID == uniqueid);
        }

        public GiactRecords? FindGiactByAddressLine1(string address, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressCurrent == address);
        }

        public GiactRecords? FindGiactByAddressCurrentPast(string addresscurrentpast, List<GiactRecords> giactrecords)
        {
            return giactrecords.FirstOrDefault(record => record.AddressPast == addresscurrentpast);
        }

        public GiactRecords? FindGiactByFullAddress(string addressline1, string city, string state, string zipcode, List<GiactRecords> giactrecords)
        { // Eppic Address1, Address2, CITY, STATE, ZIP
           return giactrecords.FirstOrDefault(record =>
                    record.AddressCurrent == addressline1 &&
                    record.CityCurrent == city &&
                    record.StateCurrent == state &&
                    record.ZipCodeCurrent == zipcode);
        }

    }
}

