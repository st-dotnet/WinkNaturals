using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WinkNaturals.Utilities.WebDrip
{
    public class BaseDrip
    {
        [IgnoreDataMember]
        public int Type { get; set; }
    }
}
