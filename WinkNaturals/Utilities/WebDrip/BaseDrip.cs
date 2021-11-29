using System.Runtime.Serialization;

namespace WinkNaturals.Utilities.WebDrip
{
    public class BaseDrip
    {
        [IgnoreDataMember]
        public int Type { get; set; }
    }
}
