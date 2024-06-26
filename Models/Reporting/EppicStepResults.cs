﻿namespace api_process_runner_api.Models.Reporting
{
    public class EppicStepResults
    {
        public string? LogID { get; set; }
        public string? LogDate { get; set; }
        public string? PersonID { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public bool Step1HospitalMatch { get; set; }
        public bool Step2GiactMatch { get; set; }
        public bool Step3PassedVerificationCheck { get; set; }
        public bool Step3aPassedOTPPhoneGiact { get; set; }
        public string? LastStepCompleted { get; set; }
        public string? Status { get; set; }
    }
}
