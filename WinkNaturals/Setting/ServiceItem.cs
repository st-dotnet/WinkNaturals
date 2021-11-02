﻿using Dapper;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Infrastructure.Services.ExigoService.Items.Requests;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities;
using WinkNaturals.Utilities.Common.Exception;

namespace WinkNaturals.Setting
{
    public class ServiceItem : IServiceItem
    {
        private readonly IOptions<ConfigSettings> _config;
        private readonly ISqlCacheService _sqlCacheService;

        public ServiceItem(IOptions<ConfigSettings> config, ISqlCacheService sqlCacheService)
        {
            _config = config;
            _sqlCacheService = sqlCacheService;

        }

        private List<Item> GetItemInformation(GetItemsRequest request, int priceTypeID)
        {
            try
            {
                var apiItems = new List<Item>();
                string cacheKey = "GetItemInformation"
                + "_W_" + request.Configuration.WarehouseID
                + "_CC_" + request.Configuration.CurrencyCode
                + "_PT_" + priceTypeID
                + "_L_" + request.LanguageID
                + "_LD_" + request.IncludeLongDescriptions.ToString()
                + "_PI_" + request.PageIndex.ToString()
                + "_PS_" + request.PageSize.ToString()
                + "_S_" + request.SortBy.ToString()
                + string.Join(",", request.ItemCodes.OrderBy(i => i)); string sorting = string.Empty;
                switch (request.SortBy)
                {
                    default:
                    case 1:
                        // Newest Arrivals
                        sorting = "i.itemId desc";
                        break;
                    case 2:
                        // Price: $ - $$
                        sorting = "ip.price, i.itemcode desc";
                        break;
                    case 3:
                        // Price: $$ - $
                        sorting = "ip.price desc, i.itemcode desc";
                        break;
                    case 4:
                        // Name: A - Z
                        sorting = "i.ItemDescription, i.itemcode";
                        break;
                }
                var orderedItems = new List<Item>();
                bool isExist = true;
                //  bool isExist = _cache.TryGetValue(cacheKey, out orderedItems);
                // _cache.Set("cacheKey", cacheKey); // if (request.IgnoreCache || !MemoryCache.Default.Contains(cacheKey))
                if (request.IgnoreCache || !isExist)
                {
                    int warehouseID = request.Configuration.WarehouseID;
                    string currencyCode = request.Configuration.CurrencyCode;
                    int languageID = request.LanguageID;
                    List<string> itemCodes = request.ItemCodes.ToList(); using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())//DAL.Sql())
                    {
                        apiItems = context.Query<Item>(@"
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
                                                    + (request.IncludeLongDescriptions
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
                            WHERE i.ItemCode in @itemCodes
                            ORDER BY " + sorting + @"
                            -- OFFSET @offset rows
                            -- FETCH NEXT @fetch rows only
                            ", new
                                                    {
                                                        warehouse = warehouseID,
                                                        currencyCode = currencyCode,
                                                        languageID = languageID,
                                                        priceTypeID = priceTypeID,
                                                        itemCodes = itemCodes,
                                                        offset = (request.PageIndex * request.PageSize),
                                                        fetch = itemCodes.Count == 1 ? 1 : request.PageSize,
                                                    }).ToList(); var length = request.ItemCodes.Count();
                        // var orderedItems = new List<Item>(); // Handle Sorting here, the sort order was based on the order of the Item Codes passed in originally
                        for (var i = 0; i < length; i++)
                        {
                            var matchingItem = apiItems.FirstOrDefault(c => c.ItemCode == request.ItemCodes[i]); if (matchingItem != null)
                            {
                                orderedItems.Add(matchingItem);
                            }
                        }
                        if (request.IgnoreCache)
                        {
                            return orderedItems;
                        }
                        else
                        {
                            //  MemoryCache.Default.Add(cacheKey, orderedItems, DateTime.Now.AddMinutes(GlobalSettings.Exigo.Api.CacheTimeout));
                            _sqlCacheService.SetCacheData(cacheKey, orderedItems, _config.Value.Api.CacheTimeout);
                            // _cache.Set(cacheKey, orderedItems, TimeSpan.FromMinutes(15));
                        }
                    }
                }
                // var data = MemoryCache.Default.Get(cacheKey) as List<Item>;
                // var data = _sqlCacheService.GetCacheData(cacheKey);

                dynamic data = _sqlCacheService.GetCacheData(cacheKey);
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void PopulateAdditionalItemData(IEnumerable<Item> items, GetItemsRequest request)
        {
            GlobalUtilities.RunAsyncTasks(
                () => { PopulateItemImages(items); },
                () => { PopulateGroupMembers(items, request); },
                () =>
                {
                    if (request.IncludeDynamicKitChildren)
                    {
                        PopulateDynamicKitMembers(items, (IOrderConfiguration)request.Configuration, request.LanguageID, request);
                    }
                }
            );
        }
        public void PopulateDynamicKitMembers(IEnumerable<Item> items, IOrderConfiguration configuration, int languageID, GetItemsRequest request)
        {
            try
            {
                // Determine if we have any dynamic kit items
                var dynamicKitMasterItemCodes = items.Where(c => c.IsDynamicKitMaster).Select(c => c.ItemCode).ToList();
                if (dynamicKitMasterItemCodes.Count == 0) return;

                using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                {

                    var allKitCategoryItems = context.Query<DynamicKitCategoryItem>(@"
                        select
                            MasterItemID = idkcm.MasterItemID,
                            DynamicKitCategoryID = idkcm.DynamicKitCategoryID,
                            DynamicKitCategory = dkc.DynamicKitCategoryDescription, 
                            Quantity = idkcm.Quantity,
                            ItemID = i.ItemID,
                            ItemCode = i.ItemCode,
                            ItemDescription = i.ItemDescription,
                            TinyImage = i.TinyImageName,
                            SmallImage = i.SmallImageName,
                            LargeImage = i.LargeImageName

                            from ItemDynamicKitCategoryMembers idkcm
                            inner join ItemDynamicKitCategories dkc
	                            on dkc.DynamicKitCategoryID = idkcm.DynamicKitCategoryID
                            inner join Items mi
	                            on idkcm.MasterItemID = mi.ItemID
	                            and mi.ItemCode in @itemcodes
                            left join ItemDynamicKitCategoryItemMembers dkitem
	                            on dkitem.DynamicKitCategoryID = idkcm.DynamicKitCategoryID
                            left join Items i
	                            on dkitem.ItemID = i.ItemID
                        ", new { itemCodes = dynamicKitMasterItemCodes }).ToList();


                    foreach (var dynamicKitMasterItemCode in dynamicKitMasterItemCodes)
                    {
                        // Get the specifc master DK item
                        var item = items.Where(c => c.ItemCode == dynamicKitMasterItemCode).FirstOrDefault();
                        if (item == null) continue;

                        // Get the appropriate kit category items for the master DK item
                        var kitCategoryItems = allKitCategoryItems.Where(c => c.MasterItemID == item.ItemID).ToList();
                        var itemCodeCategoryKeys = kitCategoryItems.Select(c => new
                        {
                            ItemCode = c.ItemCode,
                            DynamicKitCategoryID = c.DynamicKitCategoryID
                        });
                        var itemCodes = itemCodeCategoryKeys.Select(c => c.ItemCode);
                        var categories = new List<DynamicKitCategory>();

                        // Assemble a unique list of kit categories
                        foreach (var cat in kitCategoryItems)
                        {
                            if (categories.Where(c => c.DynamicKitCategoryID == cat.DynamicKitCategoryID).Count() == 0)
                            {
                                var category = new DynamicKitCategory();

                                category.Quantity = cat.Quantity;
                                category.DynamicKitCategoryDescription = cat.DynamicKitCategory;
                                category.DynamicKitCategoryID = cat.DynamicKitCategoryID;
                                categories.Add(category);
                            }
                        }

                        // Add the list of kit categories to the master DK item
                        item.DynamicKitCategories = categories;

                        // get the information for the child kit category items


                        var childrenItems = GetItems(new GetItemsRequest { Configuration = configuration, LanguageID = languageID, ItemCodes = itemCodes.ToArray() });

                        // Map the items to correct kit categories on the the master DK item
                        foreach (var citem in childrenItems)
                        {
                            var _category = itemCodeCategoryKeys.Where(c => c.ItemCode == citem.ItemCode).FirstOrDefault();
                            if (_category != null)
                            {
                                var itemCategory = item.DynamicKitCategories.FirstOrDefault(c => c.DynamicKitCategoryID == _category.DynamicKitCategoryID);

                                itemCategory.Items.Add(new DynamicKitCategoryItem
                                {
                                    ItemID = citem.ItemID,
                                    ItemDescription = citem.ItemDescription,
                                    TinyImageUrl = citem.TinyImageUrl,
                                    SmallImageUrl = citem.SmallImageUrl,
                                    LargeImageUrl = citem.LargeImageUrl,
                                    ItemCode = citem.ItemCode,
                                    Quantity = itemCategory.Quantity,
                                    ParentItemCode = dynamicKitMasterItemCode,
                                    GroupMasterItemCode = dynamicKitMasterItemCode,
                                    DynamicKitCategory = itemCategory.DynamicKitCategoryDescription,
                                    DynamicKitCategoryID = itemCategory.DynamicKitCategoryID
                                });
                            }


                            // Get the static kit members if needed
                            if (citem.ItemTypeID == 1)
                            {
                                var itemCodeAsList = new List<string>() { citem.ItemCode };
                                item.StaticKitChildren = GetStaticKitChildren(itemCodeAsList, request);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        public IEnumerable<Item> GetItems(GetItemsRequest request, bool includeItemDescriptions = true)

        {
            // If we don't have what we need to make this call, stop here.
            if (request.Configuration == null)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");

            if (request.Configuration.CategoryID == 0 && request.CategoryID == null && request.ItemCodes.Length == 0)
                throw new InvalidRequestException("ExigoService.GetItems() requires either a CategoryID or a collection of item codes."); ;


            // Set some defaults
            if (request.CategoryID == null && request.ItemCodes.Length == 0)
            {
                request.CategoryID = request.Configuration.CategoryID;
            }


            var tempCategoryIDs = new List<int>();
            var categoryIDs = new List<int>();
            if (request.CategoryID != null)
            {
                // Get all category ids underneath the request's category id
                if (request.IncludeChildCategories)
                {
                    using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                    {
                        categoryIDs.AddRange(context.Query<int>(@"
                            WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel) 
                                 AS (SELECT WebCategoryID, 
                                            WebCategoryDescription, 
                                            ParentID, 
                                            NestedLevel 
                                     FROM   WebCategories 
                                     WHERE  WebCategoryID = @masterCategoryID
                                            AND WebID = @webid
                                     UNION ALL 
                                     SELECT w.WebCategoryID, 
                                            w.WebCategoryDescription, 
                                            w.ParentID, 
                                            w.NestedLevel 
                                     FROM   WebCategories w 
                                            INNER JOIN webcat c 
                                                    ON c.WebCategoryID = w.ParentID) 
                            SELECT WebCategoryID
                            FROM   webcat
                        ", new
                        {
                            webid = _config.Value.GlobalItems.WebID,
                            masterCategoryID = request.CategoryID
                        }).ToList());
                    }
                }
                else
                {
                    categoryIDs.Add(Convert.ToInt32(request.CategoryID));
                }
            }

            // If we requested specific categories, get the item codes in the categories
            if (categoryIDs.Count > 0)
            {
                var categoryItemCodes = new List<string>();

                using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                {
                    categoryItemCodes = context.Query<string>(@"
                        SELECT DISTINCT
	                        i.ItemCode
                            ,c.SortOrder
                        FROM 
                            WebCategoryItems c
	                        INNER JOIN Items i
		                        on c.ItemID = i.ItemID
	                        INNER JOIN WebCategories w
		                        on w.WebID = c.WebID
		                        and w.WebCategoryID = c.WebCategoryID
                        WHERE 
	                        c.WebID = @webid
	                        and c.WebCategoryID in @webcategoryids
                        ORDER By c.SortOrder
                    ", new
                    {
                        webid = _config.Value.GlobalItems.WebID,
                        webcategoryids = categoryIDs
                    }).ToList();
                }

                var existingItemCodes = request.ItemCodes.ToList();
                existingItemCodes.AddRange(categoryItemCodes);
                request.ItemCodes = existingItemCodes.ToArray();
            }

            // Do a final check to ensure if the category we are looking at does not contain a item directly nested within it, we pull back the first child category
            if (request.ItemCodes.Length == 0 && request.CategoryID != null)
            {
                var tempItemCodeList = new List<string>();
                using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                {
                    tempItemCodeList = context.Query<string>(@"                
                    ;WITH 
                        webcat 
                     (
                        WebCategoryID
                        ,WebCategoryDescription
                        ,ParentID
                        ,NestedLevel
                        ,SortOrder
                     ) 
				     AS 
                     (
                        SELECT 
                            WebCategoryID 
						    ,WebCategoryDescription
						    ,ParentID 
						    ,NestedLevel
                            ,SortOrder 
					    FROM   
                            WebCategories 
					    WHERE  
                            WebCategoryID = @masterCategoryID
						    AND WebID = @webid
					    
                        UNION ALL
                         
					    SELECT 
                            w.WebCategoryID 
						    ,w.WebCategoryDescription 
						    ,w.ParentID
						    ,w.NestedLevel
                            ,w.SortOrder
					    FROM   
                            WebCategories w 
					        INNER JOIN webcat c 
						        ON c.WebCategoryID = w.ParentID
                    ) 
                    SELECT 
                        i.ItemCode
                    FROM 
                        WebCategoryItems c
	                    INNER JOIN Items i
		                    ON c.ItemID = i.ItemID
                    WHERE 
                        c.WebCategoryID = (
                                            SELECT TOP 1 
                                                WebCategoryID 
					                        FROM 
                                                webcat 
                                            WHERE 
                                                ParentID = @masterCategoryID 
					                        ORDER BY 
                                                SortOrder
                                          )
                    ORDER BY
                        c.SortOrder
                    ", new
                    {
                        webid = _config.Value.GlobalItems.WebID,
                        masterCategoryID = request.CategoryID
                    }).ToList();
                }

                request.ItemCodes = tempItemCodeList.ToArray();
            }


            // If we don't have any items, stop here.
            if (request.ItemCodes.Length == 0) yield break;

            // Ensure our language ID is pulled from the Language Cookie
            request.LanguageID = Language.GetSelectedLanguageID();

            // get the item information             
            var priceTypeID = (request.PriceTypeID > 0) ? request.PriceTypeID : request.Configuration.PriceTypeID;

            var items = includeItemDescriptions ? GetItemInformation(request, priceTypeID) : GetItemList(request, priceTypeID);

            // Populate the group members and dynamic kits
            if (items.Any())
            {
                PopulateAdditionalItemData(items, request);
            }

            if (request.SortBy == 1)
            {
                // Newest Arrivals
                items = items.OrderByDescending(x => x.ItemID).ToList();
            }
            if (request.SortBy == 2)
            {
                // Price: $ - $$
                items = items.OrderBy(x => x.Price).ToList();
            }
            else if (request.SortBy == 3)
            {
                // Price: $$ - $
                items = items.OrderByDescending(x => x.Price).ToList();
            }
            else if (request.SortBy == 4)
            {
                // Name: A - Z
                items = items.OrderBy(x => x.ItemDescription).ToList();
            }
            else
            {
                // Featured          
            }

            // Return the data
            foreach (var item in items)
            {
                yield return item;
            }
        }
        // Get Static Kit item and Children
        public List<Item> GetStaticKitChildren(List<string> itemCode, GetItemsRequest request)
        {
            var items = new List<Item>();
            using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())//DAL.Sql())
            {
                items = context.Query<Item>(@"
                        SELECT
                            ItemID = i.ItemID,
                            ItemCode = i.ItemCode,
                            ItemDescription = 
                            case 
                                when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
                                when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription) else i.ItemDescription 
	                        end,
                            Weight = i.Weight,
                            ItemTypeID = i.ItemTypeID,
                            TinyImageUrl = i.TinyImageName,
                            SmallImageUrl = i.SmallImageName,
                            LargeImageUrl = i.LargeImageName,
                            ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
                            ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
                            ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
                            ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),
                            LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
                            LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
                            LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
                            LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),
                            IsVirtual = i.IsVirtual,
                            AllowOnAutoOrder = i.AllowOnAutoOrder,
                            AllowPersonalization = i.OtherCheck1,
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
                            Auto1 = i.Auto1,
                            Auto2 = i.Auto2,
                            Auto3 = i.Auto3,
                            --Price = ip.Price,
                            --CurrencyCode = ip.CurrencyCode,
                            --BV = ip.BusinessVolume,
                            --CV = ip.CommissionableVolume,
                            --OtherPrice1 = ip.Other1Price,
                            --OtherPrice2 = ip.Other2Price,
                            --OtherPrice3 = ip.Other3Price,
                            --OtherPrice4 = ip.Other4Price,
                            --OtherPrice5 = ip.Other5Price,
                            --OtherPrice6 = ip.Other6Price,
                            --OtherPrice7 = ip.Other7Price,
                            --OtherPrice8 = ip.Other8Price,
                            --OtherPrice9 = ip.Other9Price,
                            --OtherPrice10 = ip.Other10Price,
                            Quantity = iskm.Quantity,
                            ParentItemCode = mi.ItemCode
                        FROM ItemStaticKitMembers iskm
                        INNER JOIN Items i
	                        ON i.ItemID = iskm.ItemID
                        INNER JOIN Items mi
                            ON mi.ItemID = iskm.MasterItemID
                        --INNER JOIN ItemPrices ip
                        --    ON ip.ItemID = i.ItemID
                        --    AND ip.PriceTypeID = @priceTypeID
                        INNER JOIN ItemWarehouses iw
                            ON iw.ItemID = i.ItemID
                            AND iw.WarehouseID = @warehouse                        
                        LEFT JOIN ItemLanguages il
                            ON il.ItemID = i.ItemID
                            AND il.LanguageID = @languageID
                        WHERE iskm.MasterItemID in (select ItemID from Items where ItemCode = @itemcode)
                            --AND ip.CurrencyCode = @currencyCode
                        ", new
                {
                    warehouse = request.Configuration.WarehouseID,
                    currencyCode = request.Configuration.CurrencyCode,
                    priceTypeID = request.Configuration.PriceTypeID,
                    languageID = request.LanguageID,
                    itemcode = itemCode
                }).ToList();
            }

            // Return the data
            foreach (var item in items)
            {
                // get the correct product path
                //

                item.LargeImageUrl = GlobalUtilities.GetProductImagePath(item.LargeImageUrl);
                item.SmallImageUrl = GlobalUtilities.GetProductImagePath(item.SmallImageUrl);
                item.TinyImageUrl = GlobalUtilities.GetProductImagePath(item.TinyImageUrl);
            }

            return items;
        }
        public void PopulateItemImages(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                if (!item.TinyImageUrl.Contains("productimages"))
                {
                    item.TinyImageUrl = GlobalUtilities.GetProductImagePath(item.TinyImageUrl);
                }
                if (!item.SmallImageUrl.Contains("productimages"))
                {
                    item.SmallImageUrl = GlobalUtilities.GetProductImagePath(item.SmallImageUrl);
                }
                if (!item.LargeImageUrl.Contains("productimages"))
                {
                    item.LargeImageUrl = GlobalUtilities.GetProductImagePath(item.LargeImageUrl);
                }

            }
        }
        public void PopulateGroupMembers(IEnumerable<Item> items, GetItemsRequest request)
        {
            int languageID = request.LanguageID;
            string currencyCode = request.Configuration.CurrencyCode;
            int warehouseID = request.Configuration.WarehouseID;

            try
            {
                // Determine if we have any group master items
                var groupMasterItemIDs = items.Where(c => c.IsGroupMaster).Select(c => c.ItemID).ToList();
                if (groupMasterItemIDs.Count == 0) return;

                // Get a list of group member items for all the group master items
                var itemGroupMembers = new List<ItemGroupMember>();


                using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())//DAL.Sql())
                {
                    context.Open();
                    itemGroupMembers = context.Query<Item, ItemGroupMember, ItemGroupMember>(@"
                SELECT
	                ItemCode = i.ItemCode,
	                ItemDescription = i.ItemDescription,
	                Weight = i.Weight,
	                ItemTypeID = i.ItemTypeID,
	                TinyImageUrl = i.TinyImageName,
	                SmallImageUrl = i.SmallImageName,
	                LargeImageUrl = i.LargeImageName,
	                ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),
	                LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),
	                IsVirtual = i.IsVirtual,
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
	                Auto1 = i.Auto1,
	                Auto2 = i.Auto2,
	                Auto3 = i.Auto3,
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
	                OtherPrice10 = ip.Other10Price,
                    MasterItemID = im.MasterItemID,
                    MemberDescription = im.GroupMemberDescription,
                    SortOrder = im.Priority,
                    ItemID = i.ItemID
                    FROM ItemGroupMembers im
	                inner join Items i
		                on i.ItemID = im.ItemID
	                INNER JOIN ItemPrices ip
		                ON ip.ItemID = i.ItemID
		                AND ip.PriceTypeID = @priceTypeID    
	                INNER JOIN ItemWarehouses iw
		                ON iw.ItemID = i.ItemID
		                AND iw.WarehouseID = @warehouse
					LEFT JOIN ItemLanguages il
		                ON il.ItemID = i.ItemID
						AND il.LanguageID = @languageID
					WHERE ip.CurrencyCode = @currencyCode
                        AND im.ItemID != im.MasterItemID
                        AND im.MasterItemID in @groupMasterItemIDs
                                                
                ", (Item, ItemGroupMember) =>
                    {
                        ItemGroupMember.Item = Item;
                        ItemGroupMember.ItemCode = Item.ItemCode;
                        return ItemGroupMember;
                    }, new
                    {
                        warehouse = request.Configuration.WarehouseID,
                        currencyCode = request.Configuration.CurrencyCode,
                        languageID = request.LanguageID,
                        priceTypeID = request.Configuration.PriceTypeID,
                        groupMasterItemIDs = groupMasterItemIDs
                    }, splitOn: "MasterItemID").ToList();

                    context.Close();
                }


                //bind the item group members to the group master items               
                foreach (var groupmasteritemid in groupMasterItemIDs)
                {
                    var masteritem = items.Where(c => c.ItemID == groupmasteritemid).FirstOrDefault();
                    if (masteritem == null) continue;

                    masteritem.GroupMembers = itemGroupMembers
                        .Where(c => c.MasterItemID == groupmasteritemid)
                        .OrderBy(c => c.SortOrder)
                        .ToList();

                    // populate the master item's basic details for cart purposes
                    foreach (var groupmember in masteritem.GroupMembers)
                    {
                        groupmember.Item = groupmember.Item ?? new Item();
                        groupmember.Item.ItemCode = groupmember.ItemCode;
                        groupmember.Item.GroupMasterItemCode = masteritem.ItemCode;
                    }
                }
            }
            catch { }
        }

        public List<Item> GetItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IOrderConfiguration configuration, int languageID, int _priceTypeID = 0)
        {
            dynamic results = new List<Item>();

            // If we don't have what we need to make this call, stop here.
            if (configuration == null && shoppingCartItems.Count() == 0)
                throw new InvalidRequestException("ExigoService.GetItems() requires an OrderConfiguration.");
            // Create the contexts we will use
            var priceTypeID = (_priceTypeID > 0) ? _priceTypeID : configuration.PriceTypeID;

            var itemcodes = new List<string>();

            shoppingCartItems.ToList().ForEach(c => itemcodes.Add(c.ItemCode));

            var apiItems = GetItemInformation(new GetItemsRequest { Configuration = configuration, LanguageID = languageID, ItemCodes = itemcodes.ToArray(), IgnoreCache = true }, priceTypeID);

            // Populate the group members and dynamic kits
            if (apiItems.Any())
            {
                var request = new GetItemsRequest
                {
                    LanguageID = languageID,
                    Configuration = configuration
                };
                PopulateAdditionalItemData(apiItems, request);
            }

            foreach (var cartItem in shoppingCartItems)
            {
                var apiItem = apiItems.FirstOrDefault(i => i.ItemCode == cartItem.ItemCode);

                if (apiItem != null)
                {
                    var newItem = apiItem.DeepClone();
                    newItem.ID = cartItem.ID;
                    newItem.Quantity = cartItem.Quantity;
                    newItem.ParentItemCode = cartItem.ParentItemCode;
                    newItem.GroupMasterItemCode = cartItem.GroupMasterItemCode;
                    newItem.DynamicKitCategory = cartItem.DynamicKitCategory;
                    newItem.Type = (Models.ShoppingCartItemType)cartItem.Type;
                    results.Add(newItem);
                }
            }
            return results;
        }
        private List<Item> GetItemList(GetItemsRequest request, int priceTypeID)
        {
            try
            {
                var apiItems = new List<Item>();

                int warehouseID = request.Configuration.WarehouseID;
                string currencyCode = request.Configuration.CurrencyCode;
                int languageID = request.LanguageID;
                List<string> itemCodes = request.ItemCodes.ToList();

                using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
                {
                    apiItems = context.Query<Item>(@"
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
	                            ShortDetail1 = '',
	                            ShortDetail2 = '',
	                            ShortDetail3 = '',
	                            ShortDetail4 = '',
	                            LongDetail1 = '',
	                            LongDetail2 = '',
	                            LongDetail3 = '',
	                            LongDetail4 = '',
	                            IsVirtual = i.IsVirtual,
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
					        WHERE i.ItemCode in @itemCodes
                        ", new
                    {
                        warehouse = warehouseID,
                        currencyCode = currencyCode,
                        languageID = languageID,
                        itemCodes = itemCodes,
                        priceTypeID = priceTypeID
                    }).ToList();
                }

                // Handle Sorting here, the sort order was based on the order of the Item Codes passed in originally
                var length = request.ItemCodes.Count();
                var orderedItems = new List<Item>();
                for (var i = 0; i < length; i++)
                {
                    var matchingItem = apiItems.FirstOrDefault(c => c.ItemCode == request.ItemCodes[i]);

                    if (matchingItem != null)
                    {
                        orderedItems.Add(matchingItem);
                    }
                }

                return orderedItems;
            }
            catch (Exception e)
            {
                throw;
            }
        }


    }
}
