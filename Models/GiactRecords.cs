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
        public string? AddressCurrent; // AddressLine1
        public string? CityCurrent;
        public string? StateCurrent;
        public string? ZipCodeCurrent;
        public string? AddressPast; // AddressLine19
        public string? CityPast;
        public string? StatePast;
        public string? ZipCodePast;
        public string? Status;
        public string? Classification;
        public string? NumberType;
        public string? PhoneNumber;


    }
}
