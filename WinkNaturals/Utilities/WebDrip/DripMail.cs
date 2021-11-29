using Newtonsoft.Json;
using RestSharp;
using System;

namespace WinkNaturals.Utilities.WebDrip
{
    public class DripMail
    {
        private String accountID = "8007203";
        private String apiToken = "5d3ccc527228d6b7c87c9e1379893314";
        private String apiUrl = "https://api.getdrip.com";
        private String password = "R&Rbbq2020";

        new public void Enqueue(BaseDrip data)
        {
            string fileName = string.Empty;
            string requestUrl = string.Empty;
            string requestContentFormat = string.Empty;
            switch (data.Type)
            {
                case 1:
                    requestUrl = "v3/{0}/shopper_activity/cart/";
                    requestContentFormat = "{0}";
                    break;
                case 2:
                    requestUrl = "v3/{0}/shopper_activity/order/";
                    requestContentFormat = "{0}";
                    break;
            }

            String b64 = System.Convert.ToBase64String((System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", this.apiToken, this.password))));
            var client = new RestClient(this.apiUrl);
            var request = new RestRequest(String.Format(requestUrl, this.accountID), Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", String.Format("Basic {0}", b64));
            String subscribers = String.Format(requestContentFormat, JsonConvert.SerializeObject(data));
            request.AddJsonBody(subscribers);

            // var res = client.Execute(request);
            client.ExecuteAsync(request, response =>
            {
            });
        }
    }
}