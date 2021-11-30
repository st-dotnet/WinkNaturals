using Newtonsoft.Json;
using System;
using WinkNaturals.Setting;

namespace WinkNaturals.Models
{
    public class BasePropertyBag : IPropertyBag2
    {
        public string Version { get; set; }
        public string Description { get; set; }
        public string SessionID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Expires { get; set; }



        public virtual bool IsValid()
        {
            return true;
        }
        public virtual T OnBeforeUpdate<T>(T propertyBag) where T : IPropertyBag2
        {
            return propertyBag;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}