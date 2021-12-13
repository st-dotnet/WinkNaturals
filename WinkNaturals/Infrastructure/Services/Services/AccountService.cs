using AutoMapper.Configuration;
using Dapper;
using Exigo.Api.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using PointTransactionType = WinkNaturals.Models.PointTransactionType;

namespace WinkNaturals.Infrastructure.Services.Services
{
    public class AccountService : IAccountService
    {
        public IEnumerable<PointTransaction> LoyaltyPointsService(int customerId, int pointAccountID)
        {
            try
            {
                var pointTransactions = new List<PointTransaction>();
                using (var context = DbConnection.Sql())
                {
                    pointTransactions = context.Query<PointTransaction>(@"
                    select
	                    pt.PointTransactionID,
	                    pt.CustomerID,
	                    pt.PointAccountID,
	                    pt.Amount,
	                    pt.PointTransactionTypeID,
	                    pt.TransactionDate,
	                    pt.OrderID,
	                    pt.Reference
                    from PointTransactions pt
                    where 
	                    pt.CustomerID = @customerid
	                    and pt.PointAccountID = @pointaccountid
                    order by pt.TransactionDate desc", new
                    {
                        customerid = customerId,
                        pointaccountid = pointAccountID
                    }).ToList();
                }

                // If we have any, let's use other existing methods to get the accounts and types,
                // and we'll sync them here. This will allow us to use any custom logic or rules
                // in the other methods.
                if (pointTransactions.Any())
                {
                    var accounts = GetPointAccounts().ToList();
                    var types = GetPointTransactionTypes().ToList();

                    foreach (var transaction in pointTransactions)
                    {
                        transaction.PointAccount = accounts.FirstOrDefault(a => a.PointAccountID == transaction.PointAccountID);
                        transaction.PointTransactionType = types.FirstOrDefault(a => a.PointTransactionTypeID == transaction.PointTransactionTypeID);
                        if (transaction.Amount > 0)
                            transaction.ExpirationDate = transaction.TransactionDate.AddYears(2);
                        transaction.Balance = pointTransactions.Where(x => x.PointTransactionID <= transaction.PointTransactionID).Sum(x => x.Amount);
                    }
                }
                if (pointTransactions == null) return null;
                return pointTransactions;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        private List<PointAccount> GetPointAccounts()
        {
            var pointAccounts = new List<PointAccount>();
            using (var context = DbConnection.Sql())
            {
                pointAccounts = context.Query<PointAccount>(@"
                                SELECT PointAccountID
                                      , PointAccountDescription
                                      , CurrencyCode
                                FROM PointAccounts
                    ").ToList();
            }

            if (pointAccounts == null) return null;

            return pointAccounts;
        }

        private List<PointTransactionType> GetPointTransactionTypes()
        {
            var pointTransactionTypes = new List<PointTransactionType>();
            using (var context = DbConnection.Sql())
            {
                pointTransactionTypes = context.Query<PointTransactionType>(@"
                SELECT PointTransactionTypeID
                        , PointTransactionTypeDescription
                FROM PointTransactionTypes
            ").ToList();
            }

            if (pointTransactionTypes == null) return null;

            return pointTransactionTypes;
        }
    }
}