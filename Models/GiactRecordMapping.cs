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
        public string? UniqueID { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? AddressCurrentPast { get; set; }
        public string? CityCurrentPast { get; set; }
        public string? StateCurrentPast { get; set; }
        public string? ZipCodeCurrentPast { get; set; }
        public string? Status { get; set; }
        public string? Classification { get; set; }
        public string? NumberType { get; set; }
        public string? PhoneNumber { get; set; }


    }
}
