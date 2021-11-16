using System.Net;
using System.Text;

namespace WinkNaturals.Helpers
{
    public class PostHelpers
    {
        public static string PostJson(string url, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);

            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                var response = client.UploadData(url, "POST", bytes);

                return Encoding.Default.GetString(response);
            }
        }
    }
}
