using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO.Customer
{
    public class CustomerUpdateRequest
    {
        public int CustomerId { get; set; } 
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string NewPassword { get; set; }  
        public string Url { get; set; }   
    }
}
