using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.Interfaces
{
    public interface ICustomerType
    {
        int CustomerTypeID { get; set; }
        string CustomerTypeDescription { get; set; }
    }
}
