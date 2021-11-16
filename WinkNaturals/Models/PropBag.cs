using System;

namespace WinkNaturals.Models
{
    public class PropBag
    {
        string Version { get; set; }
        string Description { get; set; }
        string SessionID { get; set; }
        DateTime CreatedDate { get; set; }
        int Expires { get; set; }

    }
}
