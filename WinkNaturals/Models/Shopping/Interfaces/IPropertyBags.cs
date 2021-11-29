using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IPropertyBags
    {
        string Version { get; set; }
        string Description { get; set; }
        string SessionID { get; set; }
        DateTime CreatedDate { get; set; }
        int Expires { get; set; }

        bool IsValid();
        T OnBeforeUpdate<T>(T propertyBag) where T : IPropertyBags;

    }
}
