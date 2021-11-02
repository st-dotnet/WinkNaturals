using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Web.Common.Utils
{
    public class ExigoConfig
    {
        public string LoginName { get; set; }

        public string Password { get; set; }

        public string CompanyKey { get; set; }

        public string SandboxID { get; set; }

        private static ExigoConfig instance;
        public static ExigoConfig Instance
        {
            get
            {
                return ExigoConfig.instance;
            }
            set
            {
                if (ExigoConfig.instance != null) return;
                ExigoConfig.instance = value;
            }
        }

    }
}
