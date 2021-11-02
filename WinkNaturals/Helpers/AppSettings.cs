using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string AuthorizeNetTestBaseUrl { get; set; }
        public string AuthorizeNetBaseUrl { get; set; }
        public string APIKey { get; set; }
        public string TransactionKey { get; set; }
    }
}
