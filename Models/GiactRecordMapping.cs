using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class GiactRecords
    {
        public string? UniqueID;
        public string? AddressLine1;
        public string? City;
        public string? State;
        public string? ZipCode;
        public string? AddressCurrentPast;
        public string? CityCurrentPast;
        public string? StateCurrentPast;
        public string? ZipCodeCurrentPast;
        public string? Status;
        public string? Classification;
        public string? NumberType;
        public string? PhoneNumber;


    }
}
