using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class SiebelRecords
    {
        public string? ProgramName;
        public string? PersonID;

        public string? ActivityCreatedDate;
        public string? ActivityType;

        [FieldOptional]
        [FieldQuoted(MultilineMode.AllowForBoth)]
        public string? ActivityDescription;
    }
}



