using System;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace GodsBlessing
{
    class APIHelper
    {
        public string PathToWebsite { get; private set; }

        private string apiKey;

        public APIHelper(string apiKey, string serverPath)
        {
            this.apiKey = apiKey;

            PathToWebsite = string.Format("{0}/stuff/{1}/", serverPath, apiKey);
        }

        public bool TestConnection()
        {
            bool wasSuccessful = true;

            try
            {
                GetData("ConfigurationHelper", "GetApplicationConfiguration", new string[] { "applicationName" });
            }
            catch
            {
                wasSuccessful = false;
            }

            return wasSuccessful;
        }

        public dynamic GetData(string className, string methodOrPropertyName)
        {
            string fullHTTPRequest = String.Format("{0}/{1}/{2}", PathToWebsite, className, methodOrPropertyName);
            string rawResponse = GetRawDataAsync(fullHTTPRequest);

            return JObject.Parse(rawResponse);
        }

        public dynamic GetData(string className, string methodOrPropertyName, string[] parameters)
        {
            string fullHTTPRequest = String.Format("{0}/{1}/{2}/{3}", PathToWebsite, className, methodOrPropertyName, String.Join("/", parameters));
            string rawResponse = GetRawDataAsync(fullHTTPRequest);

            return JObject.Parse(rawResponse);
        }

        private string GetRawDataAsync(string fullHTTPRequest)
        {
            HttpClient httpClient = new HttpClient();
            string response = ""; 

            Task.Run(async () => { response = await httpClient.GetStringAsync(fullHTTPRequest); }).Wait();

            return response;
        }
    }
}
