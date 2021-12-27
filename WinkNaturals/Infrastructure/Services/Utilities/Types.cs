using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Infrastructure.Services.Utilities
{
    public static class TypeExtensions
    {
        public static bool CanBeParsedAs<T>(this object objectToBeParsed)
        {
            try
            {
                var castedObject = Convert.ChangeType(objectToBeParsed, typeof(T));
                return castedObject != null;
            }
            catch
            {
                return false;
            }
        }
    }
}