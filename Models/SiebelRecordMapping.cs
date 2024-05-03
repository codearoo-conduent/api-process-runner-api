using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{   // RDC Updated: 4/30 to reflects the columns in 791
    [DelimitedRecord(","), IgnoreFirst(1)]
    public class SiebelRecords
    {
        public string? ProgramName;
        public string? PersonID;

        public string? ActivityCreatedDate;
        public string? ActivityType;

        [FieldOptional]
        [FieldQuoted(MultilineMode.AllowForBoth)]
        public string? ActivityDescription; // This is the field that has multiline data and has the call notes data.  Technically this will not require the use of AI, but we can infact use it.  
    }
}



