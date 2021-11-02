using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class ProductListRequest
    {
        public int categoryID { get; set; }
        public int sortBy { get; set; }

    }
}
