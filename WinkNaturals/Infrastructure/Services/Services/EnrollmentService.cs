using Dapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class EnrollmentService : IEnrollmentService
    {
		//private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

		private readonly IConfiguration _config;
        public EnrollmentService(IConfiguration config)
        {
            _config = config;
        }

        #region Public methods

        public List<EnrollmentResponse> GetItems()
        {
            try
            {
                var apiItems = new List<EnrollmentResponse>();
                GetItemsRequest request = new GetItemsRequest
                {
                    //We will be requesting three items
                    ItemCodes = new string[3]
                };
                request.ItemCodes[0] = "SK-Q1KIT3-21";
                request.ItemCodes[1] = "SK-Q1KIT2-21";
                request.ItemCodes[2] = "SK-Q1KIT1-21";
                using (var context = DbConnection.Sql())
                {
                    apiItems = context.Query<EnrollmentResponse>(@"
                			    SELECT
	                                ItemID = i.ItemID,
	                                ItemCode = i.ItemCode,
	                                ItemDescription = 
		                                case 
			                                when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
			                                when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
							                else i.ItemDescription
		                                end,
	                                Weight = i.Weight,
	                                ItemTypeID = i.ItemTypeID,
	                                TinyImageUrl = i.TinyImageName,
	                                SmallImageUrl = i.SmallImageName,
	                                LargeImageUrl = i.LargeImageName,
	                                ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                                ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                                ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                                ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),"
                       + (true
                         ? @"LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                                LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                                LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                                LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),"
                         : string.Empty)
                       + @"IsVirtual = i.IsVirtual,
	                                AllowOnAutoOrder = i.AllowOnAutoOrder,
	                                IsGroupMaster = i.IsGroupMaster,
	                                IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                                GroupMasterItemDescription = i.GroupDescription,
	                                GroupMembersDescription = i.GroupMembersDescription,
	                                Field1 = i.Field1,
	                                Field2 = i.Field2,
	                                Field3 = i.Field3,
	                                Field4 = i.Field4,
	                                Field5 = i.Field5,
	                                Field6 = i.Field6,
	                                Field7 = i.Field7,
	                                Field8 = i.Field8,
	                                Field9 = i.Field9,
	                                Field10 = i.Field10,
	                                OtherCheck1 = i.OtherCheck1,
	                                OtherCheck2 = i.OtherCheck2,
	                                OtherCheck3 = i.OtherCheck3,
	                                OtherCheck4 = i.OtherCheck4,
	                                OtherCheck5 = i.OtherCheck5,
	                                Price = ip.Price,
	                                CurrencyCode = ip.CurrencyCode,
	                                BV = ip.BusinessVolume,
	                                CV = ip.CommissionableVolume,
	                                OtherPrice1 = ip.Other1Price,
	                                OtherPrice2 = ip.Other2Price,
	                                OtherPrice3 = ip.Other3Price,
	                                OtherPrice4 = ip.Other4Price,
	                                OtherPrice5 = ip.Other5Price,
	                                OtherPrice6 = ip.Other6Price,
	                                OtherPrice7 = ip.Other7Price,
	                                OtherPrice8 = ip.Other8Price,
	                                OtherPrice9 = ip.Other9Price,
	                                OtherPrice10 = ip.Other10Price
                                FROM Items i
	                                INNER JOIN ItemPrices ip
		                                ON ip.ItemID = i.ItemID
		                                    AND ip.PriceTypeID = @priceTypeID
						                    AND ip.CurrencyCode = @currencyCode                                
	                                INNER JOIN ItemWarehouses iw
		                                ON iw.ItemID = i.ItemID
		                                    AND iw.WarehouseID = @warehouse
						            LEFT JOIN ItemLanguages il
		                                ON il.ItemID = i.ItemID
						                    AND il.LanguageID = @languageID
					            WHERE i.ItemCode in @itemCodes", new
                       {
                           warehouse = 1,
                           currencyCode = "usd",
                           languageID = 0,
                           priceTypeID = 1,
                           itemCodes = request.ItemCodes,
                       }).ToList();

                    return apiItems;
                }
            }
            catch (Exception ex)
            {
                var enrollmentResponse = new EnrollmentResponse { Success = false, ErrorMessage = ex.Message };
                return new List<EnrollmentResponse> { enrollmentResponse };
            }
        }

     

        public Task<TransactionalResponse> ProcessExigoPayment()
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}

