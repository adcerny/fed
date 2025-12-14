using Fed.Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xero.Api.Core.Model;
using Xero.Api.Core.Model.Types;
using Xero.Api.Infrastructure.Exceptions;
using FedModel = Fed.Core.Entities;
using XeroModel = Xero.Api.Core.Model;

namespace Fed.Api.External.XeroService
{
    public class XeroContactsSyncService : XeroService
    {
        private const int maxItemCodeLength = 30;
        private const int maxItemNameLength = 50;


        public XeroContactsSyncService(XeroSettings settings, ILogger logger) :
            base(settings, logger)
        {

        }
        public async Task<bool> SyncContacts(IList<FedModel.Customer> fedCustomers)
        {
            List<Contact> xeroContacts = GetXeroContacts();

            foreach (FedModel.Customer fedCustomer in fedCustomers)
            {
                if (GetFedContact(fedCustomer) == null || GetFedBillingAddress(fedCustomer) == null)
                    continue;

                var xeroContact = xeroContacts.Where(c => c.AccountNumber == fedCustomer.ShortId).FirstOrDefault();
                if (xeroContact == null)
                    await CreateNewContact(fedCustomer);
                else if (IsStale(xeroContact, fedCustomer))
                    await UpdateExistingContact(fedCustomer, xeroContact);
            }
            return true;
        }

        private List<Contact> GetXeroContacts()
        {
            List<Contact> Contacts = new List<Contact>();
            List<Contact> ContactPage = new List<Contact>();

            int i = 1;
            do
            {
                ContactPage = new List<Contact>();
                ContactPage = _api.Contacts.Page(i).FindAsync().Result.ToList();
                Contacts.AddRange(ContactPage);
                i++;
                Thread.Sleep(1000);
            } while (ContactPage.Count() > 0);

            return Contacts;
        }

        private async Task UpdateExistingContact(FedModel.Customer fedCustomer, XeroModel.Contact contact)
        {
            var fedContact = GetFedContact(fedCustomer);
            var fedBillingAddress = GetFedBillingAddress(fedCustomer);

            _logger.LogInformation($"Updating {fedBillingAddress.CompanyName} (Account# {fedCustomer.ShortId})");

            contact.Name = fedBillingAddress.CompanyName.NormaliseSpace();
            contact.FirstName = fedContact.FirstName.NormaliseSpace();
            contact.LastName = fedContact.LastName.NormaliseSpace();
            contact.EmailAddress = GetFedEmailAddress(fedContact).NormaliseSpace();
            contact.AccountNumber = fedCustomer.ShortId.NormaliseSpace();
            //contact.ContactPersons = GetXeroContactPersons(fedCustomer);
            contact.Addresses = new List<Xero.Api.Core.Model.Address>
                {
                    new Xero.Api.Core.Model.Address
                    {
                        AddressType = AddressType.PostOfficeBox,
                        AddressLine1 = fedBillingAddress.AddressLine1.NormaliseSpace(),
                        AddressLine2 = fedBillingAddress.AddressLine2.NormaliseSpace(),
                        City = fedBillingAddress.Town.NormaliseSpace(),
                        PostalCode = fedBillingAddress.Postcode.NormaliseSpace()
                    }
                };
            try
            {
                var updatedContact = await _api.Contacts.UpdateAsync(contact);
                Thread.Sleep(1000);
            }
            catch (ValidationException vex)
            {
                _logger.LogError(vex.ToString());

                //we may have a duplicate contact, add account number to make unique
                contact.Name += $" ({contact.AccountNumber})";
                var updatedContact = await _api.Contacts.UpdateAsync(contact);
                Thread.Sleep(1000);
                _logger.LogWarning($"A contact with the BillingAddress.CompanyName of {fedBillingAddress.CompanyName.NormaliseSpace()} already exists.  Contact was updated as {contact.Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating contact account for {fedBillingAddress.CompanyName} (Account# {fedCustomer.ShortId}). Error message was {ex.ToString()}");
            }
        }

        private async Task CreateNewContact(FedModel.Customer fedCustomer)
        {
            var fedContact = GetFedContact(fedCustomer);
            var fedBillingAddress = GetFedBillingAddress(fedCustomer);

            _logger.LogInformation($"Adding {fedBillingAddress.CompanyName} (Account# {fedCustomer.ShortId})");

            var xeroContact = new Contact
            {
                Name = fedBillingAddress.CompanyName.NormaliseSpace(),
                FirstName = fedContact.FirstName.NormaliseSpace(),
                LastName = fedContact.LastName.NormaliseSpace(),
                EmailAddress = GetFedEmailAddress(fedContact).NormaliseSpace(),
                AccountNumber = fedCustomer.ShortId,
                Addresses = new List<XeroModel.Address>
                {
                    new XeroModel.Address
                    {
                        AddressType = AddressType.PostOfficeBox,
                        AddressLine1 = fedBillingAddress.AddressLine1.NormaliseSpace(),
                        AddressLine2 = fedBillingAddress.AddressLine2.NormaliseSpace(),
                        City = fedBillingAddress.Town.NormaliseSpace(),
                        PostalCode = fedBillingAddress.Postcode.NormaliseSpace()
                    }
                },
                //ContactPersons = GetXeroContactPersons(fedCustomer)
            };
            try
            {
                var createdContact = await _api.Contacts.CreateAsync(xeroContact);
                Thread.Sleep(1000);
            }
            catch (ValidationException)
            {
                //we may have a duplicate contact, add account number to make unique
                xeroContact.Name += $" ({xeroContact.AccountNumber})";
                var createdContact = await _api.Contacts.CreateAsync(xeroContact);
                Thread.Sleep(1000);
                _logger.LogWarning($"A contact with the BillingAddress.CompanyName of {fedBillingAddress.CompanyName.NormaliseSpace()} already exists.  Contact was created as {xeroContact.Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating contact account for {fedBillingAddress.CompanyName} (Account# {fedCustomer.ShortId}). Error message was {ex.ToString()}");
            }

        }

        private bool IsStale(XeroModel.Contact xerocontact, FedModel.Customer fedCustomer)
        {
            var fedContact = GetFedContact(fedCustomer);
            var fedBillingAddress = GetFedBillingAddress(fedContact);
            var xeroBillingAddress = GetXeroBillingAddress(xerocontact);

            if (xerocontact.Name != fedBillingAddress.CompanyName.NormaliseSpace())
                return true;
            if (xerocontact.FirstName != fedContact.FirstName.NormaliseSpace())
                return true;
            if (xerocontact.LastName != fedContact.LastName.NormaliseSpace())
                return true;
            if (xerocontact.EmailAddress != GetFedEmailAddress(fedContact).NormaliseSpace())
                return true;
            if (xeroBillingAddress.AddressLine1 != fedBillingAddress.AddressLine1.NormaliseSpace())
                return true;
            if (xeroBillingAddress.AddressLine2 != fedBillingAddress.AddressLine2.NormaliseSpace())
                return true;
            if (xeroBillingAddress.City != (fedBillingAddress.Town.NormaliseSpace() ?? string.Empty))
                return true;
            if (xeroBillingAddress.PostalCode != fedBillingAddress.Postcode.NormaliseSpace())
                return true;
            //if (IsStale(xerocontact.ContactPersons, fedCustomer))
            //    return true;

            return false;

        }

        private bool IsStale(List<ContactPerson> contactPersons, FedModel.Customer fedCustomer)
        {
            var fedContactPersons = GetXeroContactPersons(fedCustomer);
            var xeroContactPerson = contactPersons?.FirstOrDefault();
            var fedContactPerson = fedContactPersons?.FirstOrDefault();

            if (xeroContactPerson == null && fedContactPerson == null)
                return false;
            if (xeroContactPerson == null && fedContactPerson != null)
                return true;
            if (xeroContactPerson != null && fedContactPerson == null)
                return true;
            if (xeroContactPerson.EmailAddress != fedContactPerson.EmailAddress.NormaliseSpace())
                return true;
            if (xeroContactPerson.FirstName != fedContactPerson.FirstName.NormaliseSpace())
                return true;

            return false;
        }

        private FedModel.Contact GetFedContact(FedModel.Customer c) =>
            c.Contacts.FirstOrDefault();

        private FedModel.BillingAddress GetFedBillingAddress(FedModel.Contact c) =>
            c.BillingAddresses?.Where(b => b.IsPrimary).FirstOrDefault();

        private FedModel.BillingAddress GetFedBillingAddress(FedModel.Customer c) =>
            GetFedContact(c)?.BillingAddresses?.Where(b => b.IsPrimary).FirstOrDefault();

        private string GetFedEmailAddress(FedModel.Contact c) =>
            GetFedBillingAddress(c)?.Email ?? c.Email;

        private XeroModel.Address GetXeroBillingAddress(XeroModel.Contact c) =>
            c.Addresses?.Where(a => a.AddressType == AddressType.PostOfficeBox).FirstOrDefault();

        private List<ContactPerson> GetXeroContactPersons(FedModel.Customer c)
        {
            var contact = GetFedContact(c);
            var billingAddress = GetFedBillingAddress(c);

            if (string.IsNullOrEmpty(billingAddress.Email) || billingAddress.Email == contact.Email)
                return null;

            return new List<ContactPerson>
            {
                new ContactPerson
                {
                    EmailAddress = billingAddress.Email,
                    FirstName = billingAddress.FullName,
                    IncludeInEmails = true
                }
            };

        }


    }
}


