using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Models
{
    public class BasePropertyBag: IPropertyBag2
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