using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IItemGroupMember
    {
        string ItemCode { get; set; }
        string MasterItemCode { get; set; }

        string MemberDescription { get; set; }
        int SortOrder { get; set; }

        Item Item { get; set; }
    }
}
