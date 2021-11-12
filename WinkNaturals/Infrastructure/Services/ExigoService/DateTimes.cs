using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Infrastructure.Services.ExigoService
{
    public static class DateTimes
    {
        public static DateTime BeginningOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);
        }
    }
}
