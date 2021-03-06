using System;

namespace WinkNaturals.Setting
{
    public interface IPropertyBag2
    {
        string Version { get; set; }
        string Description { get; set; }
        string SessionID { get; set; }
        DateTime CreatedDate { get; set; }
        int Expires { get; set; }
        bool IsValid();
        T OnBeforeUpdate<T>(T propertyBag2) where T : IPropertyBag2;
    }
    public interface IPropertyBags
    {
        void SetCacheSessionData(string sessionID, string data);
        string GetCacheSessionData(string sessionID);
        T GetCacheData<T>(string description) where T : IPropertyBag2;
        T UpdateCacheData<T>(T propertyBag) where T : IPropertyBag2;
        // void GetCacheData<ItemType>(string description) where ItemType : IPropertyBag2;
        T Delete<T>(T propertyBag) where T : IPropertyBag2;
    }


}
