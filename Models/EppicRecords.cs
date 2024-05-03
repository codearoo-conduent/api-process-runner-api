using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class EppicRecords
    {   // Per User Story 790 we are only supposed to be using
        // UniqueID, HomePhoneNumber, WorkPhoneNumber
        // But the next step is to perform a lookup in the Hospital Address File
        // If we don't have an Address how do we perform the lookup?
        public string? PersonID;
        public string? Phone_Number;
        public string? AddressLine1;
        public string? AddressLine2;
        public string? City;
        public string? State;
        public string? ZipCode;
    }
}
