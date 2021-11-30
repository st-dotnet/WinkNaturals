using Dapper;
using Exigo.Api.Client;
using ExigoResourceSet.Providers.Resource;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : BaseController
    {

        private readonly IWebHostEnvironment _env;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IOrderConfiguration _orderConfiguration;

        public CustomersController(IWebHostEnvironment env, IExigoApiContext exigoApiContext, IOrderConfiguration orderConfiguration)
        {
            _env = env;
            _exigoApiContext = exigoApiContext;
            _orderConfiguration = orderConfiguration;
        }

        [HttpPost("GetCustomer")]
        public async Task<IActionResult> GetCustomer()
        {
            try
            {
                var apiItems = new List<EnrollmentModel>();
                GetItemsRequest req = new GetItemsRequest
                {
                    //We will be requesting three items
                    ItemCodes = new string[3]
                };
                req.ItemCodes[0] = "SK-Q1KIT3-21";
                req.ItemCodes[1] = "SK-Q1KIT2-21";
                req.ItemCodes[2] = "SK-Q1KIT1-21";
                req.CurrencyCode = "usd";
                req.WarehouseID = 1;
                req.PriceType = 1;

                GetItemsResponse res = await _exigoApiContext.GetContext(false).GetItemsAsync(req);
                //using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                //{
                //   apiItems= context.Query<EnrollmentModel>(@"
                //			    SELECT
                //                 ItemID = i.ItemID,
                //                 ItemCode = i.ItemCode,
                //                 ItemDescription = 
                //                  case 
                //                   when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
                //                   when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
                //       else i.ItemDescription
                //                  end,
                //                 Weight = i.Weight,
                //                 ItemTypeID = i.ItemTypeID,
                //                 TinyImageUrl = i.TinyImageName,
                //                 SmallImageUrl = i.SmallImageName,
                //                 LargeImageUrl = i.LargeImageName,
                //                 ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
                //                 ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
                //                 ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
                //                 ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),"
                //      + (true
                //        ? @"LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
                //                 LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
                //                 LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
                //                 LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),"
                //        : string.Empty)
                //      + @"IsVirtual = i.IsVirtual,
                //                 AllowOnAutoOrder = i.AllowOnAutoOrder,
                //                 IsGroupMaster = i.IsGroupMaster,
                //                 IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
                //                 GroupMasterItemDescription = i.GroupDescription,
                //                 GroupMembersDescription = i.GroupMembersDescription,
                //                 Field1 = i.Field1,
                //                 Field2 = i.Field2,
                //                 Field3 = i.Field3,
                //                 Field4 = i.Field4,
                //                 Field5 = i.Field5,
                //                 Field6 = i.Field6,
                //                 Field7 = i.Field7,
                //                 Field8 = i.Field8,
                //                 Field9 = i.Field9,
                //                 Field10 = i.Field10,
                //                 OtherCheck1 = i.OtherCheck1,
                //                 OtherCheck2 = i.OtherCheck2,
                //                 OtherCheck3 = i.OtherCheck3,
                //                 OtherCheck4 = i.OtherCheck4,
                //                 OtherCheck5 = i.OtherCheck5,
                //                 Price = ip.Price,
                //                 CurrencyCode = ip.CurrencyCode,
                //                 BV = ip.BusinessVolume,
                //                 CV = ip.CommissionableVolume,
                //                 OtherPrice1 = ip.Other1Price,
                //                 OtherPrice2 = ip.Other2Price,
                //                 OtherPrice3 = ip.Other3Price,
                //                 OtherPrice4 = ip.Other4Price,
                //                 OtherPrice5 = ip.Other5Price,
                //                 OtherPrice6 = ip.Other6Price,
                //                 OtherPrice7 = ip.Other7Price,
                //                 OtherPrice8 = ip.Other8Price,
                //                 OtherPrice9 = ip.Other9Price,
                //                 OtherPrice10 = ip.Other10Price
                //                FROM Items i
                //                 INNER JOIN ItemPrices ip
                //                  ON ip.ItemID = i.ItemID
                //                      AND ip.PriceTypeID = @priceTypeID
                //          AND ip.CurrencyCode = @currencyCode                                
                //                 INNER JOIN ItemWarehouses iw
                //                  ON iw.ItemID = i.ItemID
                //                      AND iw.WarehouseID = @warehouse
                //  LEFT JOIN ItemLanguages il
                //                  ON il.ItemID = i.ItemID
                //          AND il.LanguageID = @languageID
                // WHERE i.ItemCode in @itemCodes", new
                //      {
                //          warehouse = 1,
                //          currencyCode = "usd",
                //          languageID = 0,
                //          priceTypeID = 1,
                //          itemCodes = req.ItemCodes,
                //      }).ToList();

                //}

                var result = apiItems;
                // var field = "About";
                // var cInfo = Thread.CurrentThread.CurrentUICulture;
                // var aa =Common.GetObject(field, cInfo.Name, Path.Combine(_env.ContentRootPath, "App_Data/"));
                // var aab = DbResourceProvider.GetResourceValue("Common",new CultureInfo("es-US").ToString(), field);
                // //GetResourceSetItemsRequest req = new GetResourceSetItemsRequest { SubscriptionKey= "CommonWeb", Version="2021.7.20.2",CultureCode= new CultureInfo("en-US").ToString() };
                // //Create Request 
                // // var req = new GetResourceSetsRequest();

                // //Send Request to Server and Get Response 
                // // var res = await exigoApiClient.GetResourceSetsAsync(req);

                // //var reqq = new CreateResourceSetIntItemRequest
                // //{
                // //    ResourceSetID = 1,
                // //    ResourceType = ResourceType.Image,
                // //    ResourceName = "TestImage",
                // //    Enabled = true,
                // //    Comment = "Test Comments"

                // //};

                // //////Send Request to Server and Get Response

                // //var resq = await exigoApiClient.CreateResourceSetIntItemAsync(reqq);
                // //Create Request
                //// var req3 = new GetResourceSetsRequest { ResourceSetID = 1 };

                // //Send Request to Server and Get Response
                //// var res3 = await exigoApiClient.GetResourceSetsAsync(req3);

                // //Create Request 
                // var req1 = new GetResourceSetItemsRequest
                // {
                //     ResourceSetID = 1,
                //     CultureCode = cInfo.Name
                // };

                // //Send Request to Server and Get Response 
                // var res1 = await exigoApiClient.GetResourceSetItemsAsync(req1);

                // var typeData = res1.Items.Where(x => x.ResourceTypeID != ResourceType.Text).ToList();

                // //Create Request 
                // var req2 = new GetResourceSetItemsRequest
                // {
                //     SubscriptionKey = "CommonWeb",
                //     Version = "2021.4.28.1",
                //     CultureCode = cInfo.Name
                // };

                // //Send Request to Server and Get Response 
                // var res2= await exigoApiClient.GetResourceSetItemsAsync(req2);

                // //Create Request 
                // var req4 = new GetResourceSetItemsRequest
                // {
                //     SubscriptionKey = "CommonWeb",
                //     Version = "2021.7.20.2"
                //     //CultureCode = ""
                // };

                // //Send Request to Server and Get Response 
                // var res4 = await exigoApiClient.GetResourceSetItemsAsync(req4);

                //Implementation pending
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

    }
}
