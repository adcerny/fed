using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xero.Api.Core.Model;
using Xero.Api.Core.Model.Status;

namespace Fed.Api.External.XeroService
{
    public class XeroAccountService : XeroService
    {
        public XeroAccountService(XeroSettings settings, ILogger logger) :
            base(settings, logger)
        {

        }

        public IList<Account> GetXeroAccounts(DateTime earliestCreationDate)
        {
            _logger.LogInformation($"Getting accounts...");

            List<Account> Accounts = new List<Account>();

            try
            {
                var accounts = _api.Accounts
                                   .ModifiedSince(earliestCreationDate)
                                   .FindAsync()
                                   .Result
                                   .ToList();

                _logger.LogInformation($"Found {accounts.Count()} accounts");

                return accounts.Where(a => a.Code != null).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting accounts from Xero. Error was {ex.ToString()}.");
                throw new Exception(ex.ToString());
            }
        }

        public async Task<bool> CreateAccounts(List<Account> accounts)
        {
            var exisitngAccounts = GetXeroAccounts(DateTime.MinValue);

            foreach (var account in accounts)
            {
                try
                {

                    var accountDetails = new Account
                    {
                        Code = account.Code,
                        Name = $"{account.Name} {Guid.NewGuid()}",
                        Type = account.Type,
                        Description = account.Description,
                        TaxType = account.TaxType,
                        Status = AccountStatus.Active,
                        EnablePaymentsToAccount = account.EnablePaymentsToAccount
                    };

                    var exisitngAccount = exisitngAccounts.Where(e => e.Code == account.Code && e.Status == AccountStatus.Active).FirstOrDefault();

                    if (exisitngAccount != null)
                    {
                        _logger.LogInformation($"Updating account {account.Code}");
                        accountDetails.Id = exisitngAccount.Id;
                        var updatedAccount = await _api.Accounts.UpdateAsync(accountDetails);
                        _logger.LogInformation($"Successfully updated account {account.Code}");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        _logger.LogInformation($"Adding account {account.Code}");
                        var createdAccount = await _api.Accounts.CreateAsync(accountDetails);
                        _logger.LogInformation($"Successfully added account {account.Code}");
                        Thread.Sleep(1000);
                    }
                }
                catch (Xero.Api.Infrastructure.Exceptions.ValidationException e)
                {
                    _logger.LogError(e.ToString());
                }
            }
            return true;
        }
    }
}
