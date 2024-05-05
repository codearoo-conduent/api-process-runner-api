using api_process_runner_api.Helpers.Reporting;
using api_process_runner_api.Models.Reporting;
using api_process_runner_api.Models;
using api_process_runner_api.Helpers;

namespace api_process_runner_api.Tests
{
    internal static class Tester
    {
        public static string TestCSVManagers(string logid)
        {
         EppicStepResultsManager _eppicstepresultsmanager = new EppicStepResultsManager();
         ActionConclusionManager _actionconclusionmanager = new ActionConclusionManager();
         FraudConclusionManager _fraudconclusionmanager = new FraudConclusionManager();
         VerificationConclusionManager _verificationconclusionmanager = new VerificationConclusionManager();

            var eppicStepResultRecord = new EppicStepResults
            {
                LogID = logid,
                LogDate = DateTime.Today.ToString("yyyy-MM-dd"),
                PersonID = "1234",
                PhoneNumber = "7048245555",
                AddressLine1 = "1400 S.Main ST",
                AddressLine2 = "",
                City = "Charlotte",
                State = "NC",
                Zip = "28808",
                Step1HospitalMatch = true,
                Step2GiactMatch = false,
                Step3PassedVerificationCheck = false,
                Step3aPassedOTPPhoneGiact = false,
                LastStepCompleted = "Step3",
                Status = "Testing"
            };

            var actionConclusionRecord = new ActionConclusion
            {
                PersonID = "1234",    
                AddressChanged = "Yes", 
                AddressUpdateFrom = "1200 Main ST",
                AddressUpdateTo = "1400 S. Main ST",
                CallerAuthenticated = "Yes",
                FormOfAuthentication ="OTP",
                PhoneUpdateFrom = "8033334444",
                PhoneUpdateTo = "7048245555",
                PhoneChanged = "Yes",
                ThirdPartyInvolved = "No",
                WasCallTransferred = "Yes"   
            };
            var fraudConclusionRecord = new FraudConclusion
            {
                PersonID = "1234",
                FraudConclusionNote = "This is a test",
                FraudConclusionType = "Account Takeover",
                Recommendation = "Testing"
            };
            var verificationConclusionRecord = new VerificationConclusion
            {
                PersonID = "1234",
                ActivityRelatedTo = "Testing",
                FormOfAuthentication = "ID Verification",
                PhoneNumber = "5555555555",
                VerificationsCompleted = "Yes"
            };

            for (int i = 0; i < 10; i++)
            {
                _eppicstepresultsmanager.AddOrUpdateEppicStepResult(eppicStepResultRecord); 
                _actionconclusionmanager.AddOrUpdateActionConclusion(actionConclusionRecord); 
                _fraudconclusionmanager.AddOrUpdateFraudConclusion(fraudConclusionRecord); 
                _verificationconclusionmanager.AddOrUpdateVerificationConclusion(verificationConclusionRecord); 
            }
            _eppicstepresultsmanager.WriteToCsv(Constants.UseLocalFiles);
            _actionconclusionmanager.WriteToCsv(Constants.UseLocalFiles);
            _fraudconclusionmanager.WriteToCsv(Constants.UseLocalFiles);
            _verificationconclusionmanager.WriteToCsv(Constants.UseLocalFiles);
            return "Finished Testing";
        }
    }
}
