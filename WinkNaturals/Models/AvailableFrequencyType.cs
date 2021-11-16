using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Utilities.Common;

namespace WinkNaturals.Models
{
    public class AvailableFrequencyType
    {
        private static Dictionary<Exigo.Api.Client.FrequencyType, int> FrequencyTypeBindings
        {
            get
            {
                return new Dictionary<Exigo.Api.Client.FrequencyType, int>()
                {
                    { Exigo.Api.Client.FrequencyType.Weekly, 1 },
                    { Exigo.Api.Client.FrequencyType.BiWeekly, 2 },
                    { Exigo.Api.Client.FrequencyType.Monthly, 3 },
                    { Exigo.Api.Client.FrequencyType.Quarterly, 4 },
                    { Exigo.Api.Client.FrequencyType.SemiYearly, 5 },
                    {Exigo.Api.Client.FrequencyType.Yearly, 6 },
                    { Exigo.Api.Client.FrequencyType.BiMonthly, 7 },
                    { Exigo.Api.Client.FrequencyType.EveryFourWeeks, 8 },
                    { Exigo.Api.Client.FrequencyType.EverySixWeeks, 9 },
                    { Exigo.Api.Client.FrequencyType.EveryEightWeeks, 10 },
                    { Exigo.Api.Client.FrequencyType.EveryTwelveWeeks, 11 }
                };
            }
        }



        public static int GetFrequencyTypeID(Exigo.Api.Client.FrequencyType FrequencyType)
        {
            try
            {
                return FrequencyTypeBindings.Where(c => c.Key == FrequencyType).FirstOrDefault().Value;
            }
            catch
            {
                throw new Exception("Corresponding int not found for FrequencyType {0}.".FormatWith(FrequencyType.ToString()));
            }
        }



        public static Exigo.Api.Client.FrequencyType GetFrequencyType(int FrequencyTypeID)
        {
            try
            {
                return FrequencyTypeBindings.Where(c => c.Value == FrequencyTypeID).FirstOrDefault().Key;
            }
            catch
            {
                throw new Exception("Corresponding FrequencyType not found for int {0}.".FormatWith(FrequencyTypeID));
            }
        }
    }
}
