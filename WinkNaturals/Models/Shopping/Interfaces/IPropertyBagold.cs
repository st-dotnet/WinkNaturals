using System;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IPropertyBagold
    {
        string Version { get; set; }
        string Description { get; set; }
        string SessionID { get; set; }
        DateTime CreatedDate { get; set; }
        int Expires { get; set; }

        bool IsValid();
        T OnBeforeUpdate<T>(T propertyBag) where T : IPropertyBagold;
    }
}