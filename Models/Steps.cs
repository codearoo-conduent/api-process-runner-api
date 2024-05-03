using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_process_runner_api.Models
{
    internal class Step1_Eppic_Hospital_Address_Check
    {
        public string? Eppic_PersonID { get; set; }
        public string? Status { get; set; } // Pass or Stop

    }

    internal class Step2_Eppic_Giact_Address_Check
    {
        public string? Eppic_PersonID { get; set; }

        public string? Status { get; set; } // Pass or Stop

    }

    internal class Step3_Eppic_Seibel_CallNotes
    {
        public string? Eppic_PersonID { get; set; }

        public string? Status { get; set; } // Pass or Stop

    }


}
