namespace api_process_runner_api.Models
{

    public class VerificationConclusion
    {
    public string? PersonID { get; set; }
    public string? CallerAuthenticated { get; set; }
    public string? FormOfAuthentication { get; set; }
    public string? ThirdPartyInvolved { get; set; }
    public string? WasCallTransferred { get; set; }
    public string? PhoneUpdateFrom { get; set; }
    public string? PhoneChanged { get; set; }
    public string? AddressChanged { get; set; }
    public string? AddressUpdateFrom { get; set; }
    public string? AddressUpdateTo { get; set; }
    }

    public class EppicRecordAIConclusion
    {
        public string? PersonID { get; set; }
        public string? Phone_Number { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public VerificationConclusion? VerificationConclusion { get; set; }
        public FraudConclusion? FraudConclusion { get; set; }
    }
}


