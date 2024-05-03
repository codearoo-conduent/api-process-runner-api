using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{
    public class UploadedFilesRequest
    {
        public string? GiactFilename { get; set; }
        public string? SiebelFilename { get; set; }
        public string? EppicFilename { get; set; }
        public string? AddressFilename { get; set; }
    }
}
