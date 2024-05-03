using Azure;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;


namespace api_process_runner_api.Util
{
    internal class CallLogChecker
    {
        // This function is a CallLogChecker, it allows you to detect several things. the intent and take action accordingly which 
        // Was Caller Authenticated?: Yes, Was 3rd Party Involved? No, Was this Call Transfered?: No
        // Phone # updated from 8045055319 to 2512271296
        // Address updated from 6608 zW 124TH ST  OKLAHOMA CITY OK 73142 to 205 qfGzfLlf fVE  EVERGREEN AL 36401

        private string _promptFraudConclusion = @"PersonID: {{$personid}}
        {{$query}}

         Return the Fraud Conclusion intent of the query. The Fraud Conclusion must be in the format of JSON that consists of FraudConclusionNotes, FraudConclusionType, Recommendation properties. The FraudConclusionNotes should a short summary based on your review of the query.  The FraudConclusionType should be either 'No Fraud Detected' or 'Possible Account Takeover'.  The Recommendation should be your recommendations for futher action based on your conclusions. The JSON format should be:
        [JSON]
               {{
                  'PersonID': '12345',
                  'FraudConclusionNotes': '<conclusion>',
                  'FraudConclusionType' : 'No Fraud Detected',
                  'Recommendation': <recommendation>'
               }}
        [JSON END]

        [Examples for JSON Output]
             {{ 
              'PersonID':'12345', 
             'FraudConclusionNotes': 'There are multiple red flags suggesting potential fraud, including changes in contact information, inquiries about card information and transaction history, alert updates indicating possible account takeover',
             'FraudConclusionType': 'Account Takeover'
             'Recommendation': 'Further investigation and monitoring of the account are warranted to confirm fraudulent activity.'
             }}
 
        Per user query what is the Fraud Conclusion?";

        private string _promptActionConclusion = @"PersonID: {{$personid}}
        {{$query}}

        Return the Action Conclusion intent of the query. The Acton Conclusion must be in the format of JSON that consists of PersonID, CallerAuthenticated, FormOfAuthentication, 3rdPartyInvolved, WasCallTransferred, PhoneUpdateFrom, PhoneUpdatedTo, PhoneChanged, AddressChanged, AddressUpdateFrom, AddressUpdateTo properties. The JSON format should be:

        [JSON]
              {
                  'PersonID': '12345',
                  'CallerAuthenticated': '<authenticated>',
                  'FormOfAuthentication' : '<authform>',
                  '3rdPartyInvolved': <3rdpartyinvolved>',
                  'WasCallTransferred':<calltransfered>,
                  'PhoneUpdateFrom':<phoneupdatefrom>,
                  'PhoneUpdatedTo':<phoneupdateto>,
                  'PhoneChanged': 'Yes',
                  'AddressChanged':'No',
                  'AddressUpdateFrom':<addressupdatefrom>,
                  'AddressUpdateTo':<addressupdateto>
               }
        [JSON END]

        [Examples for JSON Output]
        {
                  'PersonID': '12345',
                  'CallerAuthenticated': 'Yes',
                  'FormOfAuthentication' : 'ID Verification',
                  '3rdPartyInvolved': 'No',
                  'WasCallTransferred':'No',
                  'PhoneUpdateFrom':'8045055319',
                  'PhoneUpdatedTo':'2512271296',
                  'PhoneChanged': 'Yes',
                  'AddressChanged':'Yes',
                  'AddressUpdateFrom':'6608 zW 124TH ST  OKLAHOMA CITY',
                  'AddressUpdateTo':'205 qfGzfLlf fVE  EVERGREEN AL'
        }

        Per use query what is the Action Conclusion?";

        public async Task<string> CheckFraudIntentAsync(Kernel kernel, string personid, string query)
        {
#pragma warning disable SKEXP0010

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object", // setting JSON output mode
            };

            KernelArguments arguments2 = new(executionSettings) { { "query", query }, { "personid", personid } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                var response = await kernel.InvokePromptAsync(_promptFraudConclusion, arguments2);
                result = response.GetValue<string>() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }

        public async Task<string> CheckFraudIntent2Async(Kernel kernel, string personid, string query)
        {
            var fraudConclusionFunction = kernel.CreateFunctionFromPrompt(_promptFraudConclusion, executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 100 });

            var response = await kernel.InvokeAsync(fraudConclusionFunction, new() { ["personid"] = personid, ["query"] = query, });

            return response.GetValue<string>() ?? "";
        }

        public async Task<string> CheckActionConclusionAsync(Kernel kernel, string personid, string query)
        {
#pragma warning disable SKEXP0010

            var executionSettings = new OpenAIPromptExecutionSettings()
            {
                ResponseFormat = "json_object",
            };

            KernelArguments arguments2 = new(executionSettings) { { "query", query }, { "personid", personid } };
            string result = "";
            try
            {
                // KernelArguments arguments = new(new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" }) { { "query", query } };
                var response = await kernel.InvokePromptAsync(_promptActionConclusion, arguments2);
                result = response.ToString() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return result ?? "";
        }
    }
}

